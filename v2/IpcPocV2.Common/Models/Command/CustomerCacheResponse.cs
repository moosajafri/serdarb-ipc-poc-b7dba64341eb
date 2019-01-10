using System;
using System.Collections.Generic;

namespace IpcPocV2.Common.Models.Command
{
    [Serializable]
    public class CustomerCacheResponse : CacheResponseBase
    {
        public List<Customer> CustomerList { get; set; }

        public CustomerCacheResponse()
        {
            this.CustomerList = new List<Customer>();
            this.IsOK = false;
        }

        public override string ToString()
        {
            return "{" + $"IsOK:{this.IsOK}, ErrDesc:{this.ErrorDescription}" + "}";
        }
    }
}