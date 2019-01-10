using System;
using System.Collections.Generic;
using System.Text;

namespace IPC.CacheServer
{
    public class CacheException : Exception
    {
        public CacheException(string Message) : base(Message)
        {

        }
    }
}
