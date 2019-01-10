using System;

namespace IpcPocV2.Server.Cache.Exceptions
{
    public class CacheException : Exception
    {
        public CacheException(string message) : base(message)
        {

        }
    }
}