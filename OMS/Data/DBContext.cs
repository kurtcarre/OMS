using System;
using Microsoft.EntityFrameworkCore;
using OMS.Auth.Models;
using OMS.Models;

namespace OMS.Data
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }

        public DbSet<Member> Members { get; set; }
        public DbSet<ChildMember> ChildMembers { get; set; }
        public DbSet<MailingListEntry> MailingListEntries { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Member>(m =>
            {
                m.HasKey(mem => mem.MemberNo);
                m.HasIndex(mem => mem.LastName).HasDatabaseName("LastNameIndex");
                m.HasIndex(mem => mem.FirstName).HasDatabaseName("FirstNameIndex");
                m.ToTable("Members");
                m.Property(mem => mem.MemberNo).ValueGeneratedOnAdd();
            });

            builder.Entity<ChildMember>(cm =>
            {
                cm.HasKey(m => m.MemberNo);
                cm.ToTable("ChildMembers");
                cm.Property(m => m.MemberNo).ValueGeneratedNever();

                cm.HasOne(m => m.Member).WithOne().HasForeignKey<ChildMember>(k => k.MemberNo);
            });

            builder.Entity<MailingListEntry>(me =>
            {
                me.HasKey(m => m.Id);
                me.ToTable("MailingList");
                me.Property(m => m.Id).ValueGeneratedOnAdd();
            });

            builder.Entity<User>(u =>
            {
                u.HasKey(k => k.Id);
                u.HasIndex(i => i.UserName).HasDatabaseName("UsernameIndex").IsUnique();
                u.HasIndex(i => i.Email).HasDatabaseName("EmailIndex").IsUnique();
                u.ToTable("Users");

                u.Property(p => p.UserName).HasMaxLength(256);
                u.Property(u => u.Email).HasMaxLength(256);

                u.HasMany(r => r.Roles).WithMany(r => r.Users).UsingEntity<UserRole>(
                    ur => ur.HasOne(u => u.Role).WithMany().HasForeignKey(k => k.RoleId),
                    ur => ur.HasOne(r => r.User).WithMany().HasForeignKey(k => k.UserId),
                    ur =>
                    {
                        ur.HasKey(k => new { k.UserId, k.RoleId });
                        ur.ToTable("UserRoles");
                    });
            });

            builder.Entity<Role>(r =>
            {
                r.HasKey(k => k.Id);
                r.HasIndex(i => i.RoleName).HasDatabaseName("RoleNameIndex").IsUnique();
                r.ToTable("Roles");
            });
        }
    }
}