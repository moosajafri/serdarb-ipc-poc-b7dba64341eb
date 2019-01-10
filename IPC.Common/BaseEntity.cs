using System;

namespace IPC.Common
{
    [Serializable]
    public class BaseEntity
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public long UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}