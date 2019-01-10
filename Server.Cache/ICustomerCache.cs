using IPC.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPC.CacheServer
{
    public interface ICustomerCache
    {
        Customer GetCustomerById(int Id);

        List<Customer> GetCustomerByGUID(string GUID);

        void Add(Customer customer);
        void Add(List<Customer> CustomerList);

    }
}
