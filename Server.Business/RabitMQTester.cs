using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using IPC.Common.Helpers;

namespace IPC.BusinessServer
{
    public class RabitMQTester:IDisposable
    {
        public const string QueueIP = "ipc-queue";
        private IConnection consumerConnection = null;
        private EventingBasicConsumer consumer = null;
        bool bContinueRunning = true;

        public void StartEnqueLoop()
        {
            Task.Run(() =>
            {
                var factory = new ConnectionFactory { HostName = QueueIP };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    while (bContinueRunning)
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

        public void StartDequeLoop()
        {
            Task.Run(() =>
            {
                var factory = new ConnectionFactory { HostName = QueueIP };

                consumerConnection = factory.CreateConnection();
                var channel = consumerConnection.CreateModel();

                channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

                consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

            });
        }

        public void Dispose()
        {
            bContinueRunning = false;
            if (consumerConnection != null)
            {
                Thread.Sleep(1000);
                consumerConnection.Close();
                consumerConnection = null;
            }
        }

        
    }
}
