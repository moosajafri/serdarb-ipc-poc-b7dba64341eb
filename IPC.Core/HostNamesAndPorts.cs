namespace IPC.Core
{
    public static class HostNamesAndPorts
    {
        public const int BusinessServerPort = 10031;
        public const int CacheServerPort = 10021;
        public const int WebClientPort = 1937;
        public const int FileClientPort = 5001;
        public const int QueuePort = 5672;
        public const int DBPort = 5432;

        public const string CacheServerIP = "ipc-server-cache";
        public const string BusinessServerIP = "ipc-server-business";
        public const string WebClientIP = "ipc-client-web";
        public const string FileClientIP = "ipc-client-file";
        public const string QueueIp = "ipc-queue";
        public const string DBIp = "ipc-db";
    }
}
