using System;

namespace IpcPocV2.Common.Models.Command
{
    [Serializable]
    public class CacheResponseBase
    {
        public bool IsOK { get; set; }
        public string ErrorDescription { get; set; }

    }
}