using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IPC.Core
{
    public class ASyncChannel
    {
        #region Constants
        readonly TimeSpan SendTimeOut = TimeSpan.FromSeconds(60 * 5);
        readonly TimeSpan ReceiveTimeOut = TimeSpan.FromSeconds(60 * 5);
        #endregion Constants

        NLog.Logger logger = NLog.LogManager.GetLogger("Channel");

        #region Properties
        /// <summary>
        /// Most recent time when data was sent over this channel, server may use this time to close inactive channels
        /// </summary>
        public DateTime LastSendTime { get; private set; }

        /// <summary>
        /// Mot recent time when data was received over this channel, server may use this time to close inactive channels
        /// </summary>
        public DateTime LastReceiveTime { get; private set; }

        public DateTime LastIOTime
        {
            get
            {
                return LastReceiveTime > LastSendTime ? LastReceiveTime : LastSendTime;
            }
        }

        /// <summary>
        /// A unique id for this instance assigned by server on server side.
        /// </summary>
        public int ChannelId { get; set; }


        public Exception LastError { get; private set; }


        /// <summary>
        /// Socket object to communicate with remote peer
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// It is used to send/receive data Asynchronously
        /// </summary>
        private NetworkStream NetStream { get; set; }

        /// <summary>
        /// IPAddress of remote peer
        /// </summary>
        public string IPAddress { get; private set; }

        /// <summary>
        /// IPEndPoint of remote peer
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; private set; }

        /// <summary>
        /// It is used to receive data, it keeps track of header,body and buffer to receive data
        /// </summary>
        public TransferState ReceiveState { get; private set; }

        /// <summary>
        /// It is used to send data, it keeps track of total data-length to send, current offset from which to send data
        /// </summary>
        public TransferState SendState { get; set; }


        /// <summary>
        /// Maintains current level of socket, process business messages only from Authticated channels
        /// </summary>
        public ChannelState Level { get; set; }

        /// <summary>
        /// Reference to server to close socket object
        /// </summary>
        public IServer Server { get; set; }

        bool _isConnected = false;
        public bool IsConnected
        {
            get { return Socket.Connected && _isConnected; }

            private set { _isConnected = value; }
        }

        /// <summary>
        /// If some data receiving operation is in progress then it is True else False
        /// </summary>
        public bool IsReading { get; private set; }

        /// <summary>
        /// If some data sending operation is in progress then it is True else False
        /// </summary>
        public bool IsWriting { get; private set; }

        #endregion Properties

        #region Events
        /// <summary>
        /// Currently it is not being used, Uncomment it in data receive functions and
        /// then use it properly
        /// </summary>
        public event Action<byte[]> OnMessageReceived;

        #endregion Events


        #region Constructor_And_Initialization


        private void Initialize()
        {
            ReceiveState = new TransferState();
            ReceiveState.PrepareForHeader();
            SendState = new TransferState();
            Level = ChannelState.Initial;

            IsReading = false;
            IsWriting = false;

        }
        /// <summary>
        /// Server uses this method to create channel, Pass in connected socket only
        /// </summary>
        /// <param name="pSocket">Pass only connected socket object</param>
        public ASyncChannel(Socket pSocket)
        {

            Initialize();

            this.Socket = pSocket;

            RemoteEndPoint = this.Socket.RemoteEndPoint as IPEndPoint;
            IPAddress = RemoteEndPoint.Address.ToString();
            logger = NLog.LogManager.GetLogger(string.Format("Channel-{0}", IPAddress));

            SetSocketOptions();
            Level = ChannelState.Connected;


            this.NetStream = new NetworkStream(this.Socket, true);
            this.NetStream.WriteTimeout = (int)SendTimeOut.TotalMilliseconds;
            this.NetStream.ReadTimeout = (int)ReceiveTimeOut.TotalMilliseconds;
            IsConnected = true;

        }

        /// <summary>
        /// Use this method to Create Channel object from client side
        /// </summary>
        /// <param name="pRemoteIP">Remote Server IP/Cypher Server IP</param>
        /// <param name="pRemotePort">Remote Server Port/Cypher Server Port</param>
        public ASyncChannel(string pRemoteIP, int pRemotePort)
        {
            Initialize();
            logger = NLog.LogManager.GetLogger("Channel");

            RemoteEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(pRemoteIP), pRemotePort);
            IPAddress = pRemoteIP;

            CreateClientSocket();
        }

        private void CreateClientSocket()
        {
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetSocketOptions();
            IsConnected = false;
        }

        private void SetSocketOptions()
        {
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, SendTimeOut.Seconds);
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ReceiveTimeOut.Seconds);
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
        }

        /// <summary>
        /// Connects with Remote Server, Call this method from client side
        /// </summary>
        public bool Connect()
        {
            try
            {
                IsConnected = false;
                this.Socket.Connect(RemoteEndPoint);
                this.NetStream = new NetworkStream(this.Socket, true);
                this.NetStream.WriteTimeout = (int)SendTimeOut.TotalMilliseconds;
                this.NetStream.ReadTimeout = (int)ReceiveTimeOut.TotalMilliseconds; //NetworkStream.ReadTimeout is in milliseconds
                IsConnected = true;

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Socket_Connect_Error RemoteEndPoint {0}", RemoteEndPoint.ToString());
                LastError = ex;
            }

            return IsConnected;
        }



        public void Reconnect(int Retries, int IntervalSeconds)
        {
            do
            {
                logger.Trace("Reconnecting Retries {0}|IntervalInSeconds {1}", Retries, IntervalSeconds);

                Close();
                Initialize();
                CreateClientSocket();
                Connect();
                Retries--;
                if (IsConnected)
                    break;

                Thread.Sleep(IntervalSeconds);
            } while (Retries > 0);
        }

        #endregion Constructor_And_Initialization



        /// <summary>
        /// Synchronous send
        /// </summary>
        /// <param name="pBuffer">Data to send with first four bytes indicating length of data</param>
        public TransferState Send(byte[] pBuffer)
        {
            TransferState state = new TransferState(pBuffer, 0, pBuffer.Length);
            //Send 4-bytes as message length
            try
            {
                do
                {
                    state.TransferedBytes = this.Socket.Send(state.Buffer, state.CurrentPos, state.DataLength, SocketFlags.None, out state.SocketError);
                    logger.Trace("Transfered_Bytes {0}|Socket_Error {1}", state.TransferedBytes, state.SocketError);
                    if (state.TransferedBytes <= 0)
                    {
                        logger.Warn("Transfered_Bytes {0}|SocketErrror {1}", state.TransferedBytes, state.SocketError);
                    }

                } while (state.DataLength > 0);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return state;
        }

        /// <summary>
        /// Synchronous receive
        /// </summary>
        public void Receive()
        {
            try
            {

                do
                {
                    ReceiveState.PrepareForHeader();

                    ReceiveState.TransferedBytes = Socket.Receive(ReceiveState.Buffer, ReceiveState.CurrentPos, ReceiveState.DataLength, SocketFlags.None, out ReceiveState.SocketError);
                    logger.Trace("Bytes_Received: {0}|SocketError {1}|IsHeader=true", ReceiveState.TransferedBytes, ReceiveState.SocketError);
                } while (ReceiveState.DataLength > 0);

                ReceiveState.PrepareForBody();
                do
                {

                    ReceiveState.TransferedBytes = Socket.Receive(ReceiveState.Buffer, ReceiveState.CurrentPos, ReceiveState.DataLength, SocketFlags.None, out ReceiveState.SocketError);
                    logger.Trace("Bytes_Received: {0}|SocketError {1}", ReceiveState.TransferedBytes, ReceiveState.SocketError);
                } while (ReceiveState.DataLength > 0);
            }
            catch (Exception ex)
            {
                //TODO: We need to dispose/close this socket
                logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// It is to be used internally only by ReceiveAsync
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ReceiveAsyncCore()
        {

            while (ReceiveState.DataLength > 0)
            {
                //logger.Trace("ReceiveState: {0}", ReceiveState.ToString());
                Task timeoutTask = Task.Delay(ReceiveTimeOut);
                Task<int> readTask = NetStream.ReadAsync(ReceiveState.Buffer, ReceiveState.CurrentPos, ReceiveState.DataLength);

                Task completedTask = await Task.WhenAny(timeoutTask, readTask);
                if (completedTask == timeoutTask)
                {
                    logger.Warn("ReadAsync_timed_out {0}", IPAddress);

                    await readTask.ContinueWith(t =>
                    {
                        logger.Error(t.Exception, "readTask error {0}", IPAddress);
                    });
                    IsConnected = false;
                    NetStream.Close();
                    return false;
                }
                LastReceiveTime = DateTime.Now;
                ReceiveState.TransferedBytes = await readTask;
                if (ReceiveState.TransferedBytes == 0)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Receives data from remote socket asynchronously.
        /// </summary>
        /// <returns>Task to await</returns>
        public async Task<bool> ReceiveAsync()
        {
            if (!IsConnected)
            {
                logger.Warn("Channel.IsConnected={0}", IsConnected);
                throw new SocketException((int)SocketError.NotConnected);
            }

            bool bIsOK = false;
            IsReading = true;
            try
            {
                logger.Trace("receiving_length_prefix");

                ReceiveState.PrepareForHeader();
                if (await ReceiveAsyncCore())
                {
                    logger.Trace("HeaderReceiveState: {0}", ReceiveState.ToString());

                    ReceiveState.PrepareForBody();

                    logger.Trace("receiving_message_body");
                    if (await ReceiveAsyncCore())
                    {
                        bIsOK = true;
                        logger.Trace("BodyReceiveState: {0}", ReceiveState.ToString());
                    }
                    else
                    {
                        logger.Trace("body_receive_error");
                    }

                    // OnMessageReceived?.Invoke(ReceiveState.Buffer); Currently this is not required, 
                }
                else
                {
                    logger.Trace("header_receive_error");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                bIsOK = false;
                IsConnected = false;
            }
            finally
            {
                IsReading = false;
            }

            return bIsOK;
        }



        /// <summary>
        /// Returns object from ReceiveState
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> GetObject<T>()
        {
            //logger.Info("Deserializing_object_of_type: {0}", typeof(T).FullName);
            if (await ReceiveAsync())
            {
                return Serializer.GetObject<T>(ReceiveState.Buffer, 4);
            }

            
            return default(T);
        }


        /// <summary>
        /// Sends data from SendState buffer to remote socket
        /// </summary>
        /// <returns>True if operation succeeded else false</returns>
        public async Task<bool> SendAsync()
        {
            bool bIsOK = false;
            IsWriting = true;
            LastSendTime = DateTime.Now;
            try
            {
                //System.Runtime.CompilerServices.CallerMemberName

                SendState.CurrentPos = 0;
                logger.Trace("Sending_Data CurrentPos:[{0}]|DataLen:[{1}]", SendState.CurrentPos, SendState.DataLength);
                await NetStream.WriteAsync(SendState.Buffer, SendState.CurrentPos, SendState.DataLength);//.ConfigureAwait(false);
                logger.Trace("Sending_Data Completed");
                bIsOK = true;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                bIsOK = false;
                IsConnected = false;
            }
            finally
            {
                IsWriting = false;
            }

            return bIsOK;
        }



        /// <summary>
        ///Sends object to remote client asynchronously
        /// </summary>
        /// <param name="ObjToSend"></param>
        /// <returns></returns>
        public async Task<bool> SendObjectAsync(object ObjToSend)
        {
            if (ObjToSend == null)
                throw new ArgumentNullException("ObjToSend");

            logger.Info("Sending_Object {0}", ObjToSend.GetType().FullName);


            byte[] binaryData = Serializer.GetBytes(ObjToSend);

            SendState.SetData(binaryData);
            return await SendAsync().ConfigureAwait(false);

        }



        /// <summary>
        /// Use only for testing. To send data SendObject(...) is recomended
        /// </summary>
        /// <param name="ObjToSend">A serializeable Object </param>
        /// <returns>Returns true if data is sent successfully else returns false</returns>
        public bool SendObject(object ObjToSend)
        {
            return SendObjectAsync(ObjToSend).ConfigureAwait(false).GetAwaiter().GetResult();
           
        }

        public bool IsAlive()
        {
            
            try
            {
                logger.Trace("Last IOTime:{0}",LastIOTime.ToString("HH:mm:ss"));
                if (DateTime.Now - LastIOTime < TimeSpan.FromSeconds(10))
                    return true;

                return IsSocketConnected(this.Socket);
            }
            catch (SocketException ex)
            {
                logger.Error(ex.Message);
            }

            return false;
        }

        static bool IsSocketConnected(Socket s)
        {
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }



        #region CleanUp

        /// <summary>
        /// Closes Socket and informs server to release this channel
        /// </summary>
        public void Close()
        {
            logger.Trace("Closing socket");
            IsConnected = false;
            //Do other required work to close socket properly
            if (Server != null)//As this reference is valid for server side only, on client side this would be null
                Server.ReleaseChannel(this);
            try
            {
                if (NetStream != null)
                    NetStream.Close();
            }
            catch { }


            if (this.Socket != null)
            {
                try { this.Socket.Shutdown(SocketShutdown.Both); } catch { }
                try { this.Socket.Close(); } catch { }
            }

            SendState = null;
            ReceiveState = null;
        }

        #endregion CleanUp
    }
}
