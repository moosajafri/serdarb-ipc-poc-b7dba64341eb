using System;
using System.Threading.Tasks;

using NLog;

using IpcPocV2.Common.InterProcessCommunication;
using IpcPocV2.Common.Models.Command;
using IpcPocV2.Server.Cache.Exceptions;

namespace IpcPocV2.Server.Cache.Helpers
{
    public class ChannelHandler : IChannelHandler
    {
        ASyncChannel _channel = null;
        NLog.Logger logger = NLog.LogManager.GetLogger("ChannelHandler");

        /// <summary>
        /// This method is called by server whenever it accepts a connection
        /// </summary>
        /// <param name="channel"></param>
        public async void ProcessConnection(ASyncChannel channel)
        {
            this._channel = channel;
            try
            {
                await ProcessInternal();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        /// <summary>
        /// This method continously reads request objects from client then processes the request and sends response to client
        /// </summary>
        /// <returns></returns>
        private async Task ProcessInternal()
        {
            try
            {

                using (MappedDiagnosticsLogicalContext.SetScoped("correlationid", _channel.ChannelId))
                {
                    CacheCommandBase command = null;

                    do
                    {
                        Console.Write("Processing client Request ");
                        command = await _channel.GetObject<CacheCommandBase>();
                        if (command != null)
                        {
                            logger.Trace("Received command:{0}", command.ToString());
                            Console.WriteLine(" {0}", command.CommandType);

                            CacheResponseBase response = ProcessCommand(command);

                            logger.Trace("Sending response {0}", response.ToString());
                            await _channel.SendObjectAsync(response);
                        }

                    } while (command != null && command.CommandType != CommandType.Terminate);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "error");
                Console.WriteLine("Error:{0}", ex.Message);
            }
            finally
            {
                Release();
            }
        }

        private CacheResponseBase ProcessCommand(CacheCommandBase command)
        {
            CacheResponseBase response = null;
            switch (command.CommandType)
            {
                case CommandType.GetFileById:

                    response = GetFileById((CacheCommand)command);
                    break;

                case CommandType.GetFileByGuid:

                    response = GetFileByGUID((CacheCommand)command);
                    break;

                case CommandType.GetCustomerFiles:

                    response = GetCustomerFiles((CacheCommand)command);
                    break;

                case CommandType.GetCustomerById:

                    response = GeCustomerById((CacheCommand)command);
                    break;

                case CommandType.GetCustomerByGUID:

                    response = GetCustomerByGUID((CacheCommand)command);
                    break;

                case CommandType.AddFile:

                    response = AddFile((AddFileCommand)command);
                    break;

                case CommandType.AddCustomer:

                    response = AddCustomer((AddCustomerCommand)command);
                    break;
            }

            return response;
        }

        private FileCacheResponse GetFileById(CacheCommand command)
        {
            string fileId = "";
            int iFileId = 0;
            bool bOK = command.Parameters.TryGetValue("Id", out fileId);
            int.TryParse(fileId, out iFileId);

            var file = Program.Cache.GetFileById(iFileId);

            FileCacheResponse response = new FileCacheResponse();
            response.IsOK = true;
            if (file == null)
            {
                response.IsOK = false;
                response.ErrorDescription = $"File not found for Id:{fileId}";
            }
            else
            {
                response.FileList.Add(file);
                response.IsOK = true;
            }

            return response;
        }

        private FileCacheResponse GetFileByGUID(CacheCommand command)
        {
            string guid = "";

            bool bOK = command.Parameters.TryGetValue("GUID", out guid);

            var files = Program.Cache.GetFileByGuid(guid);

            var response = new FileCacheResponse();
            response.IsOK = true;
            if (files == null || files.Count == 0)
            {
                response.IsOK = false;
                response.ErrorDescription = $"Files not found for GUID:{guid}";
            }
            else
            {
                response.FileList.AddRange(files);
                response.IsOK = true;
            }

            return response;
        }

        private FileCacheResponse GetCustomerFiles(CacheCommand command)
        {
            string scustomerId = "";
            int iCustomerId = 0;
            bool bOK = command.Parameters.TryGetValue("Id", out scustomerId);
            int.TryParse(scustomerId, out iCustomerId);

            var flist = Program.Cache.GetCustomerFiles(iCustomerId);

            var response = new FileCacheResponse();
            response.IsOK = true;
            if (flist == null || flist.Count == 0)
            {
                response.IsOK = false;
                response.ErrorDescription = $"File not found for Customer:{scustomerId}";
            }
            else
            {
                response.FileList.AddRange(flist);
                response.IsOK = true;
            }

            return response;
        }


        private CustomerCacheResponse GeCustomerById(CacheCommand command)
        {
            CustomerCacheResponse response = new CustomerCacheResponse();

            string stringId = "";

            if (!command.Parameters.TryGetValue("Id", out stringId))
            {
                response.IsOK = false;
                response.ErrorDescription = "Prop:Id is missing in command";
                return response;
            }

            int intId = 0;
            if (!int.TryParse(stringId, out intId))
            {
                response.IsOK = false;
                response.ErrorDescription = $"Prop:Id value {stringId} is not a valid integer";
                return response;
            }

            var customer = Program.Cache.GetCustomerById(intId);

            if (customer == null)
            {
                response.IsOK = false;
                response.ErrorDescription = $"Customer with Id {intId} does not exist";
                return response;
            }

            response.CustomerList.Add(customer);
            response.IsOK = true;

            return response;
        }


        private CustomerCacheResponse GetCustomerByGUID(CacheCommand command)
        {
            CustomerCacheResponse response = new CustomerCacheResponse();

            string guid = "";

            if (!command.Parameters.TryGetValue("GUID", out guid))
            {
                response.IsOK = false;
                response.ErrorDescription = "Prop:GUID is missing in command";
                return response;
            }

            response.CustomerList = Program.Cache.GetCustomerByGUID(guid);

            if (response.CustomerList == null || response.CustomerList.Count == 0)
            {
                response.IsOK = false;
                response.ErrorDescription = $"Customers not found for GUID:{guid}";
                return response;
            }

            response.IsOK = true;

            return response;
        }

        private CacheResponseBase AddFile(AddFileCommand command)
        {
            CacheResponseBase response = new CacheResponseBase();

            try
            {
                Program.Cache.Add(command.DataList);
                response.IsOK = true;
            }
            catch (CacheException ex)
            {
                response.IsOK = false;
                response.ErrorDescription = ex.Message;
            }

            return response;
        }


        private CacheResponseBase AddCustomer(AddCustomerCommand command)
        {
            CacheResponseBase response = new CacheResponseBase();

            try
            {
                Program.Cache.Add(command.DataList);
                response.IsOK = true;
            }
            catch (CacheException ex)
            {
                response.IsOK = false;
                response.ErrorDescription = ex.Message;
            }

            return response;
        }


        private void Release()
        {
            _channel.Close();
        }

    }
}