using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IPC.Common;
using IPC.Common.Helpers;
using IPC.Core;
using IPC.Database;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IPC.BusinessServer
{
    class Program
    {
        const int MaxConcurrentConnections = 1000;

        static ASyncServer Server;
        static RabitMQTester mqManager = null;
        static void Main(string[] args)
        {
            var studentRepo = new StudentRepository();
            studentRepo.Max(x => x.Id);
            studentRepo.Where(x => (x.Name == "a" || x.Name == "b") && x.Id > 1);

            Console.Title = "Business Server 1.0.0.0";

            Thread.Sleep(10000);

            var repo = new CustomerRepository();
            while (repo.IsConnectionObjectCanNotConnect())
            {
                Thread.Sleep(2584);
            }

            mqManager = new RabitMQTester();
            mqManager.StartEnqueLoop();
            mqManager.StartDequeLoop();

            Server = new ASyncServer(MaxConcurrentConnections, HostNamesAndPorts.BusinessServerPort, typeof(IPC.BusinessServer.ChannelHandler));
            Server.StartServer();

            Console.WriteLine($"{Console.Title} is running");


            Console.WriteLine(Environment.NewLine);
            Console.ReadLine();
            Server.Dispose();
            mqManager.Dispose();
            return;
        }




    }
}
