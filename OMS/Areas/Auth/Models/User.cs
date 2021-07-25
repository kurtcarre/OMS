using System;
using System.Collections.Generic;

namespace OMS.Auth.Models
{
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public ICollection<Role> Roles { get; set; }

        public User()
        {
            Id = Guid.NewGuid().ToString();
        }

        public User(string username) : this()
        {
            UserName = username;
        }

        public override string ToString() => UserName;
    }
}