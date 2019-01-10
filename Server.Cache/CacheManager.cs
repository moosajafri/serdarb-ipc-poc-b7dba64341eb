using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPC.Common;

namespace IPC.CacheServer
{
    public class CacheManager : ICustomerCache, IFileCache
    {
        Dictionary<int, Customer> CustomerCache { get; set; }
        Dictionary<int, File> FileCache { get; set; }

        public CacheManager()
        {
            CustomerCache = new Dictionary<int, Customer>();
            FileCache = new Dictionary<int, File>();
        }

        public void LoadCustomer()
        {
            IPC.Database.CustomerRepository repo = new Database.CustomerRepository();
            repo.CreateTable();
            var list = repo.GetAll();
            foreach (var customer in list)
            {
                this.CustomerCache.Add(customer.Id, customer);
            }
        }

        public void LoadFiles()
        {
            IPC.Database.FileRepository repo = new Database.FileRepository();
            repo.CreateTable();
            var list = repo.GetAll();
            foreach (var file in list)
            {
                this.FileCache.Add(file.Id, file);
            }
        }



        public List<Customer> GetCustomerByPhoneNo(string PhoneNo)
        {
            return this.CustomerCache.Values.Where(x => x.Phone == PhoneNo).ToList();
        }

        public Customer GetCustomerById(int Id)
        {
            Customer customer = null;
            this.CustomerCache.TryGetValue(Id, out customer);
            return customer;
        }

        public List<Customer> GetCustomerByGUID(string GUID)
        {

            return this.CustomerCache.Values.Where(x => x.Guid == GUID).ToList();

        }

        public List<Customer> GetCustomerByEmail(string Eamil)
        {
            return this.CustomerCache.Values.Where(x => x.Email == Eamil).ToList();
        }


        public File GetFileById(int Id)
        {
            File file = null;
            this.FileCache.TryGetValue(Id, out file);
            return file;
        }

        public List<File> GetCustomerFiles(int CustomerId)
        {

            return this.FileCache.Values.Where(x => x.CustomerId == CustomerId).ToList();

        }

        public List<File> GetFileByGuid(string Guid)
        {
            var fileList = this.FileCache.Values.Where(x => x.Guid == Guid).ToList();
            return fileList;
        }


        public void Add(File file)
        {
            if (this.FileCache.ContainsKey(file.Id))
                throw new CacheException($"File with Id:{file.Id} already exists in Cache");

            IPC.Database.FileRepository repo = new Database.FileRepository();
            file.Id = repo.GetMaxId() + 1;
            repo.Insert(file);
            this.FileCache.Add(file.Id, file);

        }

        public void Add(List<File> FileList)
        {
            foreach (var file in FileList)
                Add(file);
        }

        public void Add(Customer customer)
        {
            if (this.CustomerCache.ContainsKey(customer.Id))
                throw new CacheException($"Customer with Id:{customer.Id} already exists in Cache");

            IPC.Database.CustomerRepository repo = new Database.CustomerRepository();
            customer.Id = repo.GetMaxId() + 1;
            repo.Insert(customer);
            this.CustomerCache.Add(customer.Id, customer);
        }

        public void Add(List<Customer> FileList)
        {
            foreach (var customer in FileList)
                Add(customer);
        }

    }



}
