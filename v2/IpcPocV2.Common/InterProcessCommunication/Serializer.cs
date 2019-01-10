using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace IpcPocV2.Common.InterProcessCommunication
{
      public class Serializer
    {
        readonly static byte[] zero_header_buffer = new byte[] { 0, 0, 0, 0 };
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Serializes object to byte array
        /// </summary>
        /// <param name="objToSerialize"></param>
        /// <returns></returns>
        public static byte[] GetBytes(object objToSerialize)
        {

            if (objToSerialize == null)
                throw new ArgumentNullException("objToSerialize");
            try
            {
                logger.Info("Serializing_object_of_type: {0}", objToSerialize.GetType().FullName);
                using (MemoryStream memStream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    
                    memStream.Write(zero_header_buffer, 0, zero_header_buffer.Length);

                    formatter.Serialize(memStream, objToSerialize);

                    byte[] objectBuffer = memStream.ToArray();

                    int dataSize = objectBuffer.Length - 4;

                    byte[] sizeBuffer = BitConverter.GetBytes(dataSize);

                    //Copy object size bytes as first 4 bytes in message buffer
                    Array.Copy(sizeBuffer, 0, objectBuffer, 0, sizeBuffer.Length);

                    return objectBuffer;

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Serialization_Error {0}", objToSerialize.ToString());
                throw;
            }

        }

        /// <summary>
        /// Deserializes object from byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DataBuffer"></param>
        /// <returns></returns>
        public static T GetObject<T>(byte[] DataBuffer,int Offset)
        {
            if (DataBuffer == null)
                throw new ArgumentNullException("pBuffer");


            logger.Info("Deserializing_object_of_type: {0}", typeof(T).FullName);
            try
            {
                using (MemoryStream ms = new MemoryStream(DataBuffer))
                {
                    ms.Position = Offset; //4 byte size is stripped
                    BinaryFormatter formatter = new BinaryFormatter();
                    
                    return (T)formatter.Deserialize(ms);

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Deserialization_Error, Cannot get object of type [{0}]", typeof(T).FullName);
                throw;
            }
        }
    }
}