using System;

using IpcPocV2.Common.Helpers;

namespace IpcPocV2.Common.Models
{
    [Serializable]
    public class File : BaseEntity
    {
        public string FileName { get; set; }
        public string Path { get; set; }

        public int CustomerId { get; set; }

        public File()
        {

        }
        public File(int id = 0)
        {
            Id = id;
            Guid = StringHelpers.GetNewUid();
            FileName = string.Format("{0}.txt", Guid);
            Path = string.Format("/Files/{0}", FileName);
            CreatedAt = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return "{" + $"Id:{Id},Guid:{Guid},FileName:{FileName},Path:{Path}" + "}";
        }
    }
}