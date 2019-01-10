using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RabbitMQ.Client;

using IpcPocV2.Common.Helpers;

namespace IpcPocV2.Server.Business.Helpers
{
    public class QueueHelper
    {
        readonly string _queueHostName;

        public QueueHelper()
        {
            _queueHostName = ConfigurationManager.AppSettings["QueueHostName"];
        }

        public void StartEnqueLoop()
        {
            Task.Run(() =>
            {
                var factory = new ConnectionFactory { HostName = _queueHostName };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    while (true)
                    {
                        var message = "Hello World! " + StringHelpers.GetNewUid();
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
                        Console.WriteLine(" [x] Sent {0}", message);

                        Thread.Sleep(1234);
                    }
                }
            });
        }
    }
}