using System;
using System.Threading;

using IpcPocV2.Common.InterProcessCommunication;
using IpcPocV2.Server.Cache.Helpers;

namespace IpcPocV2.Server.Cache
{
    class Program
    {
        public static CacheHelper Cache;
        const int MaxConcurrentConnections = 10000;

        static void Main(string[] args)
        {
            Thread.Sleep(46368);
            Console.WriteLine("///// Starting Cache Server...");

            LoadCache();

            var server = new ASyncServer(MaxConcurrentConnections, Ports.CacheServerPort, typeof(ChannelHandler));
            server.StartServer();

            Console.WriteLine(">>>>> Async channel opened for cache server!");
            Console.ReadLine();
        }

        private static void LoadCache()
        {
            Cache = new CacheHelper();
            Cache.LoadCustomers();
            Cache.LoadFiles();
            Console.WriteLine(">>>>> Cache Loaded...");
        }
    }
}
