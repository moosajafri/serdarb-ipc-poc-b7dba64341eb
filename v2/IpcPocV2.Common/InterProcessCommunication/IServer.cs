namespace IpcPocV2.Common.InterProcessCommunication
{
    public interface IServer
    {
        void ReleaseChannel(ASyncChannel channel);
    }
}