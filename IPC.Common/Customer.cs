using System;

using IPC.Common.Helpers;

namespace IPC.Common
{
    [Serializable]
    public class Customer : BaseEntity
    {
        public string Name { get; set; }
        //public string Email => string.Format("{0}@email.com", Id);
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime BornAt { get; set; }
        public Organization Organization { get; set; }
        public string NickName { get; set; }
        public int Tempdigits { get; set; }
        public Customer()
        {
            Organization = new Organization();
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

