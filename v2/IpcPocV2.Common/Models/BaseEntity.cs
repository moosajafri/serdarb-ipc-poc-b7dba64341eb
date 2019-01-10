using System;

namespace IpcPocV2.Common.Models
{
    [Serializable]
    public class BaseEntity
    {
        public int Id { get; set; }
        public string Guid { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}