using System;
using System.Configuration;
using System.Threading.Tasks;
using IpcPocV2.Common.InterProcessCommunication;
using IpcPocV2.Common.Models.Command;
using NLog;

namespace IpcPocV2.Server.Business.Helpers
{
    public class ChannelHandler : IChannelHandler
    {
        ASyncChannel _channel = null;
        ASyncChannel _cachChannel = null;
        
        NLog.Logger logger = NLog.LogManager.GetLogger("ChannelHandler");

        public ChannelHandler()
        {
            var cacheHostName = ConfigurationManager.AppSettings["CacheHostName"];
            _cachChannel = new ASyncChannel(cacheHostName, Ports.CacheServerPort);
        }

        private bool ConnectWithCache()
        {
            return _cachChannel.Connect();
        }

        public async void ProcessConnection(ASyncChannel channel)
        {
            this._channel = channel;
            try
            {
                ConnectWithCache();
                await ProcessInternal();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

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

                        Console.WriteLine("Received command {0}", command.ToString());
                        var bOK = await _cachChannel.SendObjectAsync(command);
                        if (bOK)
                        {
                            var response = await _cachChannel.GetObject<CacheResponseBase>();
                            Console.WriteLine("Sending reply {0}", response.ToString());
                            await _channel.SendObjectAsync(response);
                        }
                        else
                        {
                            var response = new CacheResponseBase()
                            {
                                IsOK = false,
                                ErrorDescription = "Cache communication error"
                            };
                            await _channel.SendObjectAsync(response);
                        }
                        

                    } while (command != null && command.CommandType != CommandType.Terminate);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "error");
                Console.WriteLine("ChandlerHandler Error:{0}", ex.Message);
            }
            finally
            {
                Release();
            }
        }

        private void Release()
        {
            try { _channel.Close(); } catch{ }
            try { _cachChannel.Close(); } catch{ }
        }
    }
}