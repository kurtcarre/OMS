using System;

namespace OMS.Auth
{
    public class User : User<string>
    {
        public User()
        {
            Id = Guid.NewGuid().ToString();
            SecurityStamp = Guid.NewGuid().ToString();
        }

        public User(string username) : this()
        {
            UserName = username;
        }
    }

    public class User<TKey> where TKey : IEquatable<TKey>
    {
        public User()
        {

        }

        public User(string username) : this()
        {
            UserName = username;
        }

        public virtual TKey Id { get; set; }

        public virtual string UserName { get; set; }

        public virtual string Email { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        public virtual string PasswordHash { get; set; }

        public virtual string SecurityStamp { get; set; }

        public virtual string ConcurrencyStamp { get; set; }

        public override string ToString() => UserName;
    }
}