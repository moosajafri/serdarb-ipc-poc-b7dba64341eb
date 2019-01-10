using System;
using System.Threading;

using IPC.Core;
using IPC.Database;

namespace IPC.CacheServer
{
    class Program
    {
        const int MaxConcurrentConnections = 10000;

        public static CacheManager Cache;
        static ASyncServer Server;
        static void Main(string[] args)
        {
            
            Console.Title = "Cache Server 1.0.0.0";

            Thread.Sleep(5000);

            var repo = new CustomerRepository();
            while (repo.IsConnectionObjectCanNotConnect())
            {
                Thread.Sleep(2584);
            }

            Console.WriteLine("Loading cache items");
            Cache = new CacheManager();
           
            Console.WriteLine("Loading customer cache");
            Cache.LoadCustomer();

            Console.WriteLine("Loading file cache");
            Cache.LoadFiles();

            Server = new ASyncServer(MaxConcurrentConnections, HostNamesAndPorts.CacheServerPort, typeof(ChannelHandler));
            Server.StartServer();

            Console.WriteLine($"{Console.Title} is running");
            Console.ReadLine();
        }
    }
}
