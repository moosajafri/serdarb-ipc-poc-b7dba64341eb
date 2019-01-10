using System;

using IpcPocV2.Common.Helpers;

namespace IpcPocV2.Common.Models
{
    [Serializable]
    public class Customer : BaseEntity
    {
        public string Name { get; set; }
        public string Email => string.Format("{0}@email.com", Id);
        public string Phone { get; set; }
        public DateTime BornAt { get; set; }
        
        public Customer()
        {

        }

        public Customer(int id = 0)
        {
            Id = id;
            Guid = StringHelpers.GetNewUid();
            CreatedAt = DateTime.UtcNow;
            BornAt = new DateTime(new Random().Next(1940, 2010), new Random().Next(1, 12), new Random().Next(1, 25));
            Phone = new Random().Next(123456789, 987654321).ToString();
            Name = StringHelpers.GetNewUid().Substring(0, 10) + " " + StringHelpers.GetNewUid().Substring(0, 10);
        }

        public override string ToString()
        {
            return "{" + $"Id:{Id},Guid:{Guid},Email:{Email},Phone:{Phone}" + "}";
        }
    }
}