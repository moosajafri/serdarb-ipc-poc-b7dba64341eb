namespace IpcPocV2.Common.InterProcessCommunication
{
    public interface IChannelHandler
    {
        void ProcessConnection(ASyncChannel channel);
    }
}