﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using IpcPocV2.Common.Models;
using IpcPocV2.Common.Models.Command;

namespace IpcPocV2.Common.InterProcessCommunication.Clients
{
     /// <summary>
    /// This class can be used from anywhere from mvc or from business layer or from anywhere you want
    /// </summary>
    public class CacheClient : IDisposable
    {
        ASyncChannel channel = null;
        private bool isDisposed = false;

        public CacheClient(string CacheServerIP, int CacheServerPort)
        {
            channel = new ASyncChannel(CacheServerIP, CacheServerPort);
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
                throw new ObjectDisposedException("CacheClient");

            if (this.channel != null)
            {
                Close();
                channel = null;
            }
            isDisposed = true;

        }
    }
}