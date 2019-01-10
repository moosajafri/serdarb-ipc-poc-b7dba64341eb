using System;
using System.Collections.Generic;

namespace IpcPocV2.Common.Models.Command
{
    [Serializable]
    public class CacheCommand: CacheCommandBase
    {
        public Dictionary<string, string> Parameters { get; set; }

        public CacheCommand()
        {
            Parameters = new Dictionary<string, string>();
        }

        public CacheCommand(CommandType cmdType) : this()
        {
            this.CommandType = cmdType;
            Parameters = new Dictionary<string, string>();
        }

        public CacheCommand(CommandType cmdType, int Id) : this(cmdType)
        {
            this.Parameters.Add("Id", Id.ToString());
        }

        public void AddParameter(string Key, string Value)
        {
            Parameters.Add(Key, Value);
        }
    }
}