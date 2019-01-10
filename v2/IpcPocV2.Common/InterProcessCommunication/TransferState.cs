using System;
using System.Net.Sockets;

namespace IpcPocV2.Common.InterProcessCommunication
{
    public class TransferState
    {
        #region Constants
        public const int Default_Buffer_Size = 1024;
        public const int Header_Size = 4;

        #endregion Constants

        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private int _bytes_transfered = 0;
        private byte[] _buffer = null;

        /// <summary>
        /// Buffert to store data to send/receive across socket
        /// </summary>
        public byte[] Buffer
        {
            get { return _buffer; }
            set { _buffer = value; }
        }

        /// <summary>
        /// Current Position in Buffer to send/receive data
        /// </summary>
        public int CurrentPos { get; set; }

        /// <summary>
        ///Total No. of bytes to send/receive across socket
        /// </summary>
        public int DataLength { get; set; }


        /// <summary>
        /// It is True when receiving first four bytes of a message, which show length of rest of message.
        /// To receive body it becomes false.
        /// </summary>
        public bool IsReceivingHeader { get; set; }

        /// <summary>
        /// SocketError during last socket send/receive operation using on this object
        /// </summary>
        public SocketError SocketError;

        /// <summary>
        /// No. of bytes sent/received during last IO on this object
        /// </summary>
        public int TransferedBytes
        {
            get { return _bytes_transfered; }

            set
            {
                _bytes_transfered = value;
                CurrentPos += _bytes_transfered;
                DataLength -= _bytes_transfered;
            }
        }

        /// <summary>
        /// Cretes TransferState object with default buffer size
        /// </summary>
        public TransferState()
        {
            _buffer = new byte[Default_Buffer_Size];
            IsReceivingHeader = true;
            CurrentPos = 0;
            DataLength = Header_Size;
        }

        /// <summary>
        /// Creates TransferState Object and initializes it with passed in parameters
        /// </summary>
        /// <param name="pBuffer"></param>
        /// <param name="pCurrentPos"></param>
        /// <param name="pDataLen"></param>
        public TransferState(byte[] pBuffer, int pCurrentPos, int pDataLen)
        {
            this._buffer = pBuffer;
            this.CurrentPos = pCurrentPos;
            this.DataLength = pDataLen;
        }


        /// <summary>
        /// If internal buffer is smaller than passed length, makes it larger to the passed in byte length
        /// </summary>
        /// <param name="pNewBufferSize"></param>
        public void AdjustBuffer(int pNewBufferSize)
        {
            pNewBufferSize += CurrentPos;
            if (_buffer.Length < pNewBufferSize)
            {
                logger.Info("Adjusting buffer");
                byte[] new_buffer = new byte[pNewBufferSize];
                Array.Copy(_buffer, 0, new_buffer, 0, CurrentPos);
                _buffer = new_buffer;
            }
        }

        /// <summary>
        /// Sets data in buffer to send to remote client
        /// </summary>
        /// <param name="Data"></param>
        public void SetData(byte[] Data)
        {
            CurrentPos = 0;
            AdjustBuffer(Data.Length);
            Array.Copy(Data, 0, this._buffer, 0, Data.Length);
            DataLength = Data.Length;
        }

        /// <summary>
        /// Sets buffer large enough to receive message body data, sets IsReceiveHeader to false
        /// </summary>
        public void PrepareForBody()
        {
            int data_len = BitConverter.ToInt32(_buffer, 0);
            logger.Info("Body_data_len {0}", data_len);
            AdjustBuffer(data_len);
            DataLength = data_len;
            IsReceivingHeader = false;
        }

        /// <summary>
        /// Prepares current object to receive header of message
        /// </summary>
        public void PrepareForHeader()
        {
            DataLength = Header_Size;
            IsReceivingHeader = true;
            CurrentPos = 0;
        }

        public override string ToString()
        {
            return string.Format("Buffer.Len {0}|CPos {1}|DataLen {2}", Buffer.Length, CurrentPos, DataLength);
        }

    }
}