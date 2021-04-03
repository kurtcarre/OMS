using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OMS.Auth.EntityFrameworkCore
{
    public class AuthUserContext<TUser> : AuthUserContext<TUser, string> where TUser : User
    {
        public AuthUserContext(DbContextOptions options) : base(options)
        {

        }

        protected AuthUserContext()
        {

        }
    }

    public class AuthUserContext<TUser, TKey> : AuthUserContext<TUser, TKey, UserClaim<TKey>, UserLogin<TKey>>
        where TUser : User<TKey>
        where TKey : IEquatable<TKey>
    {
        public AuthUserContext(DbContextOptions options) : base(options)
        {

        }

        protected AuthUserContext()
        {

        }
    }

    public class AuthUserContext<TUser, TKey, TUserClaim, TUserLogin> : DbContext
        where TUser : User<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : UserClaim<TKey>
        where TUserLogin : UserLogin<TKey>
    {
        public AuthUserContext(DbContextOptions options) : base(options)
        {

        }

        protected AuthUserContext()
        {

        }

        public virtual DbSet<TUser> Users { get; set; }
        public virtual DbSet<TUserClaim> UserClaims { get; set; }
        public virtual DbSet<TUserLogin> UserLogins { get; set; }

        private StoreOptions GetStoreOptions() => this.GetService<IDbContextOptions>().Extensions.OfType<CoreOptionsExtension>().FirstOrDefault()?.ApplicationServiceProvider
            ?.GetService<IOptions<AuthOptions>>()?.Value.Stores;

        private class PersonalDataConverter : ValueConverter<string, string>
        {
            public PersonalDataConverter(IPersonalDataProtector protector) : base(s => protector.Protect(s), s => protector.UnProtect(s), default)
            {

            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            StoreOptions storeOptions = GetStoreOptions();
            int maxKeyLength = storeOptions?.MaxKeyLength ?? 0;
            bool encryptPersonalData = storeOptions?.ProtectPersonalData ?? false;
            PersonalDataConverter converter = null;

            builder.Entity<TUser>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.UserName).HasDatabaseName("UserNameIndex").IsUnique();
                b.HasIndex(u => u.Email).HasDatabaseName("EmailIndex");
                b.ToTable("Users");
                b.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();

                b.Property(u => u.UserName).HasMaxLength(256);
                b.Property(u => u.Email).HasMaxLength(256);

                if (encryptPersonalData)
                {
                    converter = new PersonalDataConverter(this.GetService<IPersonalDataProtector>());
                    var personalDataProps = typeof(TUser).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ProtectedPersonalDataAttribute)));
                    foreach(var p in personalDataProps)
                    {
                        if (p.PropertyType != typeof(string))
                            throw new InvalidOperationException("Only strings can be protected!");

                        b.Property(typeof(string), p.Name).HasConversion(converter);
                    }
                }

                b.HasMany<TUserClaim>().WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
                b.HasMany<TUserLogin>().WithOne().HasForeignKey(ul => ul.UserId).IsRequired();
            });

            builder.Entity<TUserClaim>(b =>
            {
                b.HasKey(uc => uc.Id);
                b.ToTable("UserClaims");
            });

            builder.Entity<TUserLogin>(b =>
            {
                b.HasKey(l => new { l.LoginProvider, l.ProviderKey });

                if(maxKeyLength > 0)
                {
                    b.Property(l => l.LoginProvider).HasMaxLength(maxKeyLength);
                    b.Property(l => l.ProviderKey).HasMaxLength(maxKeyLength);
                }

                b.ToTable("UserLogins");
            });
        }
    }
}