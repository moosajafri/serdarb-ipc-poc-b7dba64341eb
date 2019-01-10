using IPC.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPC.Cache
{
    /// <summary>
    /// This class can be used from anywhere from mvc or from business layer or from anywhere you want
    /// </summary>
    public class BusinessServerClient:IDisposable
    {
        IPC.Core.ASyncChannel channel = null;
        private bool isDisposed = false;

        public BusinessServerClient(string ServerIP, int ServerPort)
        {
            channel = new Core.ASyncChannel(ServerIP, ServerPort);
        }

        /// <summary>
        /// After instantiating CacheClient object please call this method
        /// </summary>
        public bool Connect()
        {
           return channel.Connect();
        }

        public async Task<CustomerCacheResponse> GetCustomerById(int Id)
        {
            CacheCommand command = new CacheCommand(CommandType.GetCustomerById, Id);
            await channel.SendObjectAsync(command);
            return await channel.GetObject<CustomerCacheResponse>();
        }

        public async Task<CustomerCacheResponse> GetCustomerByGUID(string GUID)
        {
            CacheCommand command = new CacheCommand(CommandType.GetCustomerByGUID);
            command.AddParameter("GUID", GUID);
            await channel.SendObjectAsync(command);
            return await channel.GetObject<CustomerCacheResponse>();
        }


        public async Task<FileCacheResponse> GetFileById(int Id)
        {
            CacheCommand command = new CacheCommand(CommandType.GetFileById, Id);
            await channel.SendObjectAsync(command);
            return await channel.GetObject<FileCacheResponse>();
        }

        public async Task<FileCacheResponse> GetCustomerFiles(int CustomerId)
        {
            CacheCommand command = new CacheCommand(CommandType.GetCustomerFiles, CustomerId);
            await channel.SendObjectAsync(command);
            return await channel.GetObject<FileCacheResponse>();
        }


        public async Task<FileCacheResponse> GetFileByGUID(string GUID)
        {
            CacheCommand command = new CacheCommand(CommandType.GetFileByGuid);
            command.AddParameter("GUID", GUID);
            await channel.SendObjectAsync(command);
            return await channel.GetObject<FileCacheResponse>();
        }


        public async Task<CacheResponseBase> Add(List<File> FileList)
        {
            AddFileCommand command = new AddFileCommand();
            command.DataList = FileList;
            await channel.SendObjectAsync(command);
            return await channel.GetObject<CacheResponseBase>();
        }


        public async Task<CacheResponseBase> Add(File File)
        {
            AddFileCommand command = new AddFileCommand();
            command.DataList.Add(File);
            await channel.SendObjectAsync(command);
            return await channel.GetObject<CacheResponseBase>();
        }

        public async Task<CacheResponseBase> Add(List<Customer> CustomerList)
        {
            AddCustomerCommand command = new AddCustomerCommand();
            command.DataList = CustomerList;
            await channel.SendObjectAsync(command);
            return await channel.GetObject<CacheResponseBase>();
        }

        public async Task<CacheResponseBase> Add(Customer Customer)
        {
            AddCustomerCommand command = new AddCustomerCommand();
            command.DataList.Add(Customer);
            await channel.SendObjectAsync(command);
            return await channel.GetObject<CacheResponseBase>();
        }


        public void Close()
        {
            channel.Close();
        }

        public void Dispose()
        {
            if (isDisposed)
                throw new ObjectDisposedException("BusinessServerClient");
            if(this.channel!=null)
            {
                this.channel.Close();
                isDisposed = true;
            }
        }
    }

}
