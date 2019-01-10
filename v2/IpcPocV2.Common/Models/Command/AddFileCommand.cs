using System;
using System.Collections.Generic;

namespace IpcPocV2.Common.Models.Command
{
    [Serializable]
    public class AddFileCommand:CacheCommandBase
    {
        public List<File> DataList { get; set; }

        public AddFileCommand()
        {
            DataList = new List<File>();
            this.CommandType = CommandType.AddFile;
        }

        public void Add(File file)
        {
            DataList.Add(file);
        }
    }
}