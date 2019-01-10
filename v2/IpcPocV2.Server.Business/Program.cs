using System;
using System.Threading;
using System.Threading.Tasks;

using IpcPocV2.Common.InterProcessCommunication;
using IpcPocV2.Server.Business.Helpers;

namespace IpcPocV2.Server.Business
{
    public class Program
    {
        const int MaxConcurrentConnections = 1000;

        static ASyncServer Server;

        public static void Main(string[] args)
        {
            Thread.Sleep(17711);
            Console.WriteLine("///// Starting Business Server...");

            DoQueueSamples();

            DoDbSamples();

            Server = new ASyncServer(MaxConcurrentConnections, Ports.BusinessServerPort, typeof(ChannelHandler));
            Server.StartServer();

            Console.WriteLine(">>>>> Async channel opened for business server!");
            Console.ReadLine();
        }

        private static void DoDbSamples()
        {
            Thread.Sleep(6765);
            Console.WriteLine(">>>>> Started increasing DB records!");

            Task.Run(() =>
            {
                var dbHelper = new DbHelper();
                dbHelper.CreateTables();

                while (true)
                {
                    var random = new Random();
                    var total = random.Next(1, 1000);
                    dbHelper.InsertCustomerRecords(total);
                    Console.WriteLine("<<<<<< Inserted " + total + " customer!");

                    total = random.Next(1, 1000);
                    dbHelper.InsertFileRecords(total);
                    Console.WriteLine("<<<<<< Inserted " + total + " file!");
                }
            });
        }

        private static void DoQueueSamples()
        {
            Thread.Sleep(4181);
            Console.WriteLine(">>>>> Started publishing queue messages!");

            var helper = new QueueHelper();
            helper.StartEnqueLoop();
        }
    }
}
