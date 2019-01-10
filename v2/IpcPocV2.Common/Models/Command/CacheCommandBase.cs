using System;

namespace IpcPocV2.Common.Models.Command
{
    [Serializable]
    public class CacheCommandBase
    {
        public CommandType CommandType { get; set; }

        public override string ToString()
        {
            return CommandType.ToString();
        }
    }
}