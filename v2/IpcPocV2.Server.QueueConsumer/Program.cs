using System;
using System.Configuration;
using System.Text;
using System.Threading;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IpcPocV2.Server.QueueConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(28657);
            Console.WriteLine("///// Starting Queue Consumer ...");

            var host = ConfigurationManager.AppSettings["QueueHostName"];
            var factory = new ConnectionFactory { HostName = host };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                Console.WriteLine("channel.IsOpen > " + channel.IsOpen);
                //channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

                Console.WriteLine("SHOULD START CONSUMING ...");
                Console.ReadLine();
            }

            Console.ReadLine();
        }
    }
}
