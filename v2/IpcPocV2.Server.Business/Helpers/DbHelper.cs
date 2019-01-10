using System.Collections.Generic;

using IpcPocV2.Common.Models;
using IpcPocV2.Data.Repositories;

namespace IpcPocV2.Server.Business.Helpers
{
    public class DbHelper
    {
        public CustomerRepository CustomerRepository { get; set; }
        public FileRepository FileRepository { get; set; }

        public DbHelper()
        {
            CustomerRepository = new CustomerRepository();
            FileRepository = new FileRepository();
        }

        public void InsertCustomerRecords(int recordCount)
        {
            var items = new List<Customer>();

            var maxId = CustomerRepository.GetMaxId() + 1;

            var last = recordCount + maxId;
            for (var i = maxId; i < last; i++)
            {
                var customer = new Customer(i);
                items.Add(customer);
                CustomerRepository.Insert(customer);
            }
        }

        public void InsertFileRecords(int recordCount)
        {
            var items = new List<File>();
            var maxId = FileRepository.GetMaxId() + 1;

            var last = recordCount + maxId;
            for (var i = maxId; i < last; i++)
            {
                var file = new File(i);
                items.Add(file);
                FileRepository.Insert(file);
            }
        }

        public void CreateTables()
        {
            var customerRepo = new CustomerRepository();
            customerRepo.CreateDatabaseIfNotExists();
            customerRepo.CreateTable();

            var fileRepo = new FileRepository();
            fileRepo.CreateTable();
        }
    }
}