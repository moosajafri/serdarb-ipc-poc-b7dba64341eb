

using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IPC.Core;
using IPC.Common;

namespace IPC.BusinessServer
{
    /// <summary>
    /// Business logic should be written in this class for each command type
    /// </summary>
    public class ChannelHandler : IChannelHandler
    {
        ASyncChannel _channel = null;
        ASyncChannel _cachChannel = null;


        NLog.Logger logger = NLog.LogManager.GetLogger("ChannelHandler");

        public ChannelHandler()
        {
            _cachChannel = new ASyncChannel(HostNamesAndPorts.CacheServerIP, HostNamesAndPorts.CacheServerPort);
        }

        private bool ConnectWithCache()
        {
            return _cachChannel.Connect();
        }

        /// <summary>
        /// This method is called by server whenever it accepts a connection
        /// </summary>
        /// <param name="channel"></param>
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
                        

                    } while (command != null && command.CommandType != Common.CommandType.Terminate);
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
