using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IPC.Core
{
    /// <summary>
    /// This class is used by Server and Clinet to send receive objects
    /// </summary>
    public class Channel
    {
        private const int MetaLengthCount = 4;

        NLog.Logger logger = NLog.LogManager.GetLogger("Channel");

        /// <summary>
        /// Socket object used in communication
        /// </summary>
        Socket socket = null;

        /// <summary>
        /// Received data is stored in this buffer
        /// </summary>
        private byte[] buffer = new byte[1024 * 4];

        /// <summary>
        /// Actual length of received data
        /// </summary>
        private int receivedDataLength;


        private string serverIP;
        private int serverPort;

        /// <summary>
        /// This constructor is used by client to start connection process with server
        /// </summary>
        /// <param name="ServerIP"></param>
        /// <param name="Port"></param>
        public Channel(string ServerIP, int Port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverIP = ServerIP;
            serverPort = Port;
        }


        /// <summary>
        /// This constructor is called by server to pass in connected socket
        /// </summary>
        /// <param name="sock"></param>
        public Channel(Socket sock)
        {
            socket = sock;
        }

        private bool Receive()
        {

            int readCount = 0;
            int offset = 0;
            int remainingCount = MetaLengthCount;
            bool bOK = false;
            SocketError sockError;

            try
            {
                while (remainingCount > 0)
                {
                    readCount = socket.Receive(buffer, offset, remainingCount, SocketFlags.None, out sockError);
                    offset += readCount;
                    remainingCount -= readCount;
                    if (sockError != SocketError.Success)
                    {
                        throw new Exception("Socket reading error");
                    }
                }

                int messageLen = BitConverter.ToInt32(buffer, 0);

                readCount = 0;
                offset = 0;
                remainingCount = messageLen;
                receivedDataLength = messageLen;

                while (remainingCount > 0)
                {
                    readCount = socket.Receive(buffer, offset, remainingCount, SocketFlags.None);
                    offset += readCount;
                    remainingCount -= readCount;
                }
                bOK = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "socket reading error");
            }

            return bOK;
        }

        private bool Send(byte[] buffer)
        {

            int sendCount = 0;
            int offset = 0;
            int remainingCount = buffer.Length;
            bool bOK = false;


            try
            {
                while (remainingCount > 0)
                {
                    sendCount = socket.Send(buffer, offset, remainingCount, SocketFlags.None);
                    offset += sendCount;
                    remainingCount -= sendCount;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "socket reading error");
            }

            return bOK;
        }

        /// <summary>
        /// Call it form client side to connect with server
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            bool bOK = false;
            try
            {
                socket.Connect(IPAddress.Parse(serverIP), serverPort);
                bOK = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while connecting to server");
            }

            return bOK;
        }

        /// <summary>
        /// Receives data on socket and deserializes object from received data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetObject<T>()
        {
            if (!Receive())
            {
                return default(T);
            }

            return Serializer.GetObject<T>(this.buffer,0);

        }

        /// <summary>
        /// Sends passed object across socket
        /// </summary>
        /// <param name="objToSend"></param>
        /// <returns></returns>
        public bool SendObject(object objToSend)
        {

            byte[] binaryData = null;
            bool bOK = false;

            try
            {
                binaryData = Serializer.GetBytes(objToSend);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "object serialization error");
                return false;
            }

            if (binaryData != null)
            {
                bOK = Send(binaryData);
            }

            return bOK;
        }


        public void Close()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

    }
}
