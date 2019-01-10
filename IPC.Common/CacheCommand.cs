using System;
using System.Collections.Generic;
using System.Text;

namespace IPC.Common
{
    [Serializable]
    public class CacheCommandBase
    {
        public CommandType CommandType { get; set; }

        public override string ToString()
        {
            return CommandType.ToString();
        }
    }

    [Serializable]
    public class CacheCommand: CacheCommandBase
    {
      
        public Dictionary<string, string> Parameters { get; set; }

        public CacheCommand()
        {
            Parameters = new Dictionary<string, string>();
        }


        public CacheCommand(CommandType cmdType) : this()
        {
            this.CommandType = cmdType;
            Parameters = new Dictionary<string, string>();
        }

        public CacheCommand(CommandType cmdType, int Id) : this(cmdType)
        {
            this.Parameters.Add("Id", Id.ToString());
        }

        public void AddParameter(string Key, string Value)
        {
            Parameters.Add(Key, Value);
        }
    }

    [Serializable]
    public class AddFileCommand:CacheCommandBase
    {
        public List<File> DataList { get; set; }

        public AddFileCommand()
        {
            DataList = new List<File>();
            this.CommandType = CommandType.AddFile;
        }

        public void Add(File file)
        {
            DataList.Add(file);
        }
    }


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

          


    [Serializable]
    public class CacheResponseBase
    {
        public bool IsOK { get; set; }
        public string ErrorDescription { get; set; }

    }

    [Serializable]
    public class FileCacheResponse : CacheResponseBase
    {

        public List<File> FileList { get; set; }

        public FileCacheResponse()
        {
            this.FileList = new List<File>();
            this.IsOK = false;
        }

        public override string ToString()
        {
            return "{" + $"IsOK:{this.IsOK}, ErrDesc:{this.ErrorDescription}" + "}";
        }
    }

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
