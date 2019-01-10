namespace IpcPocV2.Client.Web
{
    public interface IHostNameValues
    {
        string CacheHostName { get; set; }
        string BusinessHostName { get; set; }
        string FileWebApiHostName { get; set; }
    }

    public class HostNameValues : IHostNameValues
    {
        public string CacheHostName { get; set; }
        public string BusinessHostName { get; set; }
        public string FileWebApiHostName { get; set; }

        public HostNameValues(string cacheHostName, string businessHostName, string fileWebApiHostName)
        {
            CacheHostName = cacheHostName;
            BusinessHostName = businessHostName;
            FileWebApiHostName = fileWebApiHostName;
        }
    }
}