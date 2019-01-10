using System;
using System.Collections.Generic;

namespace IpcPocV2.Common.Models.Command
{
    [Serializable]
    public class AddCustomerCommand : CacheCommandBase
    {
        public List<Customer> DataList { get; set; }

        public AddCustomerCommand()
        {
            DataList = new List<Customer>();
            this.CommandType = CommandType.AddCustomer;
        }

        public void Add(Customer customer)
        {
            this.DataList.Add(customer);
        }
    }
}