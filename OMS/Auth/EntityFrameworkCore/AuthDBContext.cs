using System;
using Microsoft.EntityFrameworkCore;

namespace OMS.Auth.EntityFrameworkCore
{
    public class AuthDBContext : AuthDBContext<User, string>
    {
        public AuthDBContext(DbContextOptions options) : base(options)
        {

        }

        protected AuthDBContext()
        {

        }
    }

    public class AuthDBContext<TUser> : AuthDBContext<TUser, string> where TUser : User
    {
        public AuthDBContext(DbContextOptions options) : base(options)
        {

        }

        protected AuthDBContext()
        {

        }
    }

    public class AuthDBContext<TUser, TKey> : AuthDBContext<TUser, TKey, UserClaim<TKey>, UserLogin<TKey>>
        where TUser : User<TKey>
        where TKey : IEquatable<TKey>
    {
        public AuthDBContext(DbContextOptions options) : base(options)
        {

        }

        protected AuthDBContext()
        {

        }
    }

    public abstract class AuthDBContext<TUser, TKey, TUserClaim, TUserLogin> : AuthUserContext<TUser, TKey, TUserClaim, TUserLogin>
        where TUser : User<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : UserClaim<TKey>
        where TUserLogin : UserLogin<TKey>
    {
        public AuthDBContext(DbContextOptions options) : base(options)
        {

        }

        protected AuthDBContext()
        {

        }
        // ADD ROLE HERE
    }
}