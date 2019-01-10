using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CacheTest
{
    class Program
    {
        const int BusinessServerPort = 10031;
        const int CacheServerPort = 10021;
        const string CacheServerIP = "ipc-server-cache";
        const string BusinessServerIP = "ipc-server-business";

        static void Main(string[] args)
        {
            Console.Title = "BusinessServer Client Tests 1.0.0.0";

            IPC.Database.CustomerRepository repo = new IPC.Database.CustomerRepository();

            repo.DropTable();
            repo.CreateTable();

            repo.Insert(new IPC.Common.Customer()
            {
                BornAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy =689595,
                UpdatedBy= 689595,
                Guid = "ddr-123-45f-abc-123-45f-34rf-123",
                Id = 8975,
                Name = "Moosa",
                Phone = "123-456",
                Email = "moosa-yolo@abc.com"
                ,
                Organization = new IPC.Common.Organization()
                {
                    Id = 765,
                    Name = "yhyhyhy org"
                }
            }, 45342);
            repo.SelectAsync(null).Wait();
            var selectReturnObj = Task.Run(async () => await repo.SelectAsync(null)).GetAwaiter().GetResult();
            //bool bc2 = repo.TableExists("customer");

            var totalItems = repo.Count();
            //Debug.Assert(totalItems == 3);
            //var maxVal = repo.Max(null, y => y.Id);
            //Debug.Assert(maxVal == 23232);

            //var minVal = repo.Min(null, y => y.Id);
            //Debug.Assert(minVal == 23);
            //var sumVal = repo.Sum(null, y => y.Id);
            //Debug.Assert(sumVal ==23232+56+23);
            //var items = repo.Select(null);
            
            //Debug.Assert(items.GetType() == typeof(List<IPC.Common.Customer>) );
            //Debug.Assert(items.Count == 3);
            
            //var item = repo.SelectById(23232);
            //Debug.Assert(item.GetType() == typeof(IPC.Common.Customer));
            //Debug.Assert(item.Id==23232);

            return;
            BusinessServerTest();

            //CacheClientTest();

            // ASyncChannelTest();

            Console.ReadLine();

        }

        static async void BusinessServerTest()
        {
            IPC.Cache.CacheClient client = new IPC.Cache.CacheClient(BusinessServerIP, BusinessServerPort);
            client.Connect();

            Console.WriteLine("Running customer tests");
            for (int i = 1; i < 10; i++)
            {
                var customerResponse = await client.GetCustomerById(i);

                if (customerResponse.IsOK)
                {
                    Console.WriteLine(customerResponse.CustomerList[0].ToString());
                }
                else
                {
                    Console.WriteLine(customerResponse.ErrorDescription);
                }
            }

            Console.WriteLine("Running file tests");
            for (int i = 1; i < 10; i++)
            {
                var response = await client.GetFileById(i);

                if (response.IsOK)
                {
                    Console.WriteLine(response.FileList[0].ToString());
                }
                else
                {
                    Console.WriteLine(response.ErrorDescription);
                }
            }
            client.Close();
            Console.WriteLine("End of business server tests");
        }

        static async void CacheClientTest()
        {
            Console.WriteLine("Testing CacheClient class");

            IPC.Cache.CacheClient client = new IPC.Cache.CacheClient(CacheServerIP, BusinessServerPort);
            client.Connect();
            var customerResponse = await client.GetCustomerById(1);
            if (customerResponse.IsOK)
            {
                Console.WriteLine(customerResponse.CustomerList[0].ToString());
            }

            var fileResponse = await client.GetFileById(1);
            if (fileResponse.IsOK)
            {
                Console.WriteLine(fileResponse.FileList[0].ToString());
            }

            IPC.Common.File file = new IPC.Common.File(1001);

            var addFileResponse = await client.Add(file);
            if (addFileResponse.IsOK)
            {
                Console.WriteLine($"File {file.Id} added to cache");
            }

            fileResponse = await client.GetFileById(1001);
            if (fileResponse.IsOK)
            {
                Console.WriteLine("Refetched added file {0}", fileResponse.FileList[0].ToString());
            }


            IPC.Common.Customer customer = new IPC.Common.Customer(1001);

            var addresponse2 = await client.Add(customer);
            if (addresponse2.IsOK)
            {
                Console.WriteLine($"Customer {customer.Id} added to cache");
            }

            customerResponse = await client.GetCustomerById(1001);
            if (customerResponse.IsOK)
            {
                Console.WriteLine("Refetched added customer {0}", customerResponse.CustomerList[0].ToString());
            }


            client.Close();
            Console.WriteLine("End of CacheClient class testing");
        }

        static async void ASyncChannelTest()
        {
            try
            {
                IPC.Core.ASyncChannel channel = new IPC.Core.ASyncChannel(CacheServerIP, CacheServerPort);
                channel.Connect();

                IPC.Common.CacheCommand command = new IPC.Common.CacheCommand(IPC.Common.CommandType.GetFileById);
                command.Parameters.Add("Id", "1");


                await channel.SendObjectAsync(command);
                var response = await channel.GetObject<IPC.Common.FileCacheResponse>();

                if (response.IsOK)
                {
                    Console.WriteLine(response.ToString());
                }

                channel.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
