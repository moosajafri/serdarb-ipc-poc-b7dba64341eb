﻿using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace IpcPocV2.Common.InterProcessCommunication
{
    public static class SocketExtentions
    {

        public static void SetSocketKeepAliveValues(this Socket instance, int KeepAliveTime, int KeepAliveInterval)
        {

            //sock.SetSocketKeepAliveValues(2000, 1000);
            //KeepAliveTime: default value is 2hr
            //KeepAliveInterval: default value is 1s and Detect 5 times

            //the native structure
            //struct tcp_keepalive {
            //ULONG onoff;
            //ULONG keepalivetime;
            //ULONG keepaliveinterval;
            //};

            int size = Marshal.SizeOf(new uint());
            byte[] inOptionValues = new byte[size * 3]; // 4 * 3 = 12
            bool OnOff = true;

            BitConverter.GetBytes((uint)(OnOff ? 1 : 0)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)KeepAliveTime).CopyTo(inOptionValues, size);
            BitConverter.GetBytes((uint)KeepAliveInterval).CopyTo(inOptionValues, size * 2);

            instance.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
    }
}