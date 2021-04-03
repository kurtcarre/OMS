using System;
using Microsoft.EntityFrameworkCore;
using OMS.Auth;
using OMS.Models;
using OMS.Auth.EntityFrameworkCore;

namespace OMS.Data
{
    public class DBContext : AuthDBContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }

        public DbSet<Member> Members { get; set; }
        public DbSet<ChildMember> ChildMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Member>(m =>
            {
                m.HasKey(mem => mem.MemberNo);
                m.HasIndex(mem => mem.LastName).HasDatabaseName("LastNameIndex");
                m.HasIndex(mem => mem.FirstName).HasDatabaseName("FirstNameIndex");
                m.ToTable("Members");
                m.Property(mem => mem.MemberNo).ValueGeneratedOnAdd();

                m.HasOne<ChildMember>().WithOne(r => r.Member).HasForeignKey<ChildMember>(k => k.MemberNo);
            });

            builder.Entity<ChildMember>(cm =>
            {
                cm.HasKey(m => m.MemberNo);
                cm.ToTable("ChildMembers");
                cm.Property(m => m.MemberNo).ValueGeneratedNever();
            });
        }
    }
}