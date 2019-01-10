using System;
using System.Collections.Generic;

namespace IpcPocV2.Common.Models.Command
{
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
}