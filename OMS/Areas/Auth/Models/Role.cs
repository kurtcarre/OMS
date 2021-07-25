using System;
using System.Collections.Generic;

namespace OMS.Auth.Models
{
    public class Role
    {
        public string Id { get; set; }
        public string RoleName { get; set; }

        public ICollection<User> Users { get; set; }

        public Role()
        {
            Id = Guid.NewGuid().ToString();
        }

        public Role(string name) : this()
        {
            RoleName = name;
        }

        public override string ToString() => RoleName;
    }
}