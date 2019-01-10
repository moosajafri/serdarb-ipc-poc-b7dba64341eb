using IPC.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IPC.Core
{

    public class ASyncServer : IServer,IDisposable
    {


        const int starting_time_from_now = 1000 * 20;
        const int signal_interval_time = 1000 * 20;

        private Socket ServerSocket;


        ConcurrentDictionary<int, ASyncChannel> Channels = new ConcurrentDictionary<int, ASyncChannel>();

        private NLog.Logger logger = NLog.LogManager.GetLogger("ASyncServer");
        private SocketAsyncEventArgs _acceptEventArg;


        private Semaphore MaxConcurrentClients;

        private int _noOfConnectedSockets;

        private int _clientId;


        /// <summary>
        /// Listening Port for server socket
        /// </summary>
        public int ListeningPort { get; private set; }//It was 7777 for main server
        /// <summary>
        /// No of connections in waiting que
        /// </summary>
        public int BackLog { get; set; }
        /// <summary>
        /// Maximum no of concurrent connected clients
        /// </summary>
        public int MaxConcurrentClientsCount { get; private set; }
        Type channelHandlerType;
        /// <summary>
        /// Dead socket monitoring timer
        /// </summary>
        private Timer SocketMonitoringTimer { get; set; }

        /// <summary>
        /// Constructor to create server object
        /// </summary>
        /// <param name="maxConcurrentClients">Maximum No of clients which may remain connected concurrently</param>
        /// <param name="listeningPort">Listening Port of server</param>
        public ASyncServer(int maxConcurrentClients, int listeningPort, Type ChannelHandlerType)
        {
            this.ListeningPort = listeningPort;
            this.MaxConcurrentClientsCount = maxConcurrentClients;

            MaxConcurrentClients = new Semaphore(MaxConcurrentClientsCount, MaxConcurrentClientsCount);

            _acceptEventArg = new SocketAsyncEventArgs();
            _acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);


            SocketMonitoringTimer = new Timer(new TimerCallback(OnSocketTimerTick), null, starting_time_from_now, signal_interval_time);
            this.channelHandlerType = ChannelHandlerType;
        }

        /// <summary>
        /// Call back for dead socket monitoring timer
        /// </summary>
        /// <param name="state"></param>
        private void OnSocketTimerTick(object state)
        {
            logger.Trace("Checking dead sockets");

            if (Channels.Count == 0) return;
            ASyncChannel[] arrChannel = new ASyncChannel[Channels.Count + 5];
            Channels.Values.CopyTo(arrChannel, 0);

            logger.Trace("Connected socket count: {0}", Channels.Count);

            SocketMonitoringTimer.Change(Timeout.Infinite, Timeout.Infinite);

            for (int i = 0; i < arrChannel.Length; i++)
            {
                try
                {
                    if (arrChannel[i] == null) continue;
                    ASyncChannel channel = arrChannel[i];
                    if (!channel.IsAlive())
                    {
                        channel.Close();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }

            SocketMonitoringTimer.Change(starting_time_from_now, signal_interval_time);
        }

        /// <summary>
        /// Starts accepting client connections
        /// </summary>
        public void StartServer()
        {
            try
            {


                logger.Info("Starting ServerSocket to listen on Port [{0}]", ListeningPort);

                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                logger.Info("created the socket");
                ServerSocket.Bind(new IPEndPoint(IPAddress.Any, ListeningPort));
                logger.Info("binded port");
                ServerSocket.Listen(BackLog);
                logger.Info("listening");
                StartAccept();
                logger.Info("ServerSocket is ready to accept client connections");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Server_Socket_Start_Error");
            }
        }

        /// <summary>
        /// Accepts client connections asynchronously
        /// </summary>
        private void StartAccept()
        {
            try
            {
                _acceptEventArg.AcceptSocket = null;

                MaxConcurrentClients.WaitOne();

                bool willRaiseEvent = ServerSocket.AcceptAsync(_acceptEventArg);
                if (!willRaiseEvent)
                {
                    ProcessAccept(_acceptEventArg);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        /// <summary>
        /// Callback for AcceptAsync call on Listening Socket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        /// <summary>
        /// Processes accepted socket connection
        /// </summary>
        /// <param name="e"></param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                Socket socket = e.AcceptSocket;

                Task.Factory.StartNew((clientSocket) =>
                  {
                      try
                      {
                          Socket client = clientSocket as Socket;

                          IPEndPoint ep = client.RemoteEndPoint as IPEndPoint;
                          Interlocked.Increment(ref _noOfConnectedSockets);
                          logger.Trace("Accepted_Client_Connection {0}|Connected_Clients {1}", ep.Address.ToString(), _noOfConnectedSockets);
                          ASyncChannel channel = new ASyncChannel(client);
                          channel.Server = this;
                          channel.ChannelId = NextClientId();

                          AddChannel(channel);
                          IChannelHandler handler = (IChannelHandler)System.Activator.CreateInstance(this.channelHandlerType);
                          handler.ProcessConnection(channel);
                      }
                      catch (Exception ex)
                      {
                          logger.Error(ex, "Error while processing channel");
                      }
                  }, socket);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Accepted_Client_Processing_Error");
            }
            finally
            {
                StartAccept();
            }
        }

        /// <summary>
        /// Adds accepted socket to internal channl collection
        /// </summary>
        /// <param name="channel"></param>
        private void AddChannel(ASyncChannel channel)
        {
            try
            {
                IPEndPoint ep = channel.RemoteEndPoint;
                int retry_count = 5;

                while (retry_count > 0 && !Channels.TryAdd(channel.ChannelId, channel))
                {
                    logger.Error("cannot_add_channel_to_list {0}|{1}", channel.ChannelId, ep.Address.ToString());
                    retry_count--;
                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }


        /// <summary>
        /// Whenever a client connection is closed, this method must be called, it releases channel form internal collection
        /// </summary>
        public void ReleaseChannel(ASyncChannel channel)
        {
            try
            {
                Interlocked.Decrement(ref _noOfConnectedSockets);
                MaxConcurrentClients.Release();
                ASyncChannel xchannel;
                int retry_count = 5;
                while (!Channels.TryRemove(channel.ChannelId, out xchannel))
                {
                    logger.Trace("Channel [Id:{0}, IP:{1}] was not removed from channel list", channel.ChannelId, channel.IPAddress);
                    System.Threading.Thread.Sleep(100);
                    retry_count--;
                    if (retry_count <= 0)
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Release_channel_error {0}", channel.IPAddress);
            }

        }

        /// <summary>
        /// Returns a unique interger id to be assigned to client
        /// </summary>
        /// <returns></returns>
        private int NextClientId()
        {
            return Interlocked.Increment(ref _clientId);
        }

        public void Dispose()
        {
            if (this.ServerSocket != null)
            {
                try
                {
                    this.ServerSocket.Shutdown(SocketShutdown.Both);
                    this.ServerSocket.Close();
                }
                catch { }

                foreach (var kv in Channels)
                {
                    try { kv.Value.Close(); } catch { }


                }
            }
        }
    }
}
