using System;
using System.Collections.Generic;
using System.Text;

namespace IPC.Core
{
    public interface IChannelHandler
    {
        void ProcessConnection(ASyncChannel channel);
    }

    public interface IServer
    {
        void ReleaseChannel(ASyncChannel channel);
    }
}
