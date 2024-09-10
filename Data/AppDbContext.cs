using Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

namespace Data

{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }  
        public DbSet<ChatHistory> ChatHistories { get; set; }  
        public DbSet<Role> Roles { get; set; }  
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Statistics> Statistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<ChatHistory>().ToTable("ChatHistories");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<UserToken>().ToTable("UserTokens");
            modelBuilder.Entity<Statistics>().ToTable("Statistics");


            modelBuilder.Entity<User>()
                .HasKey(u => u.userId);

            modelBuilder.Entity<ChatHistory>()
                .HasKey(c => c.historyId);

            modelBuilder.Entity<Role>()
              .HasKey(r => r.roleId);

            modelBuilder.Entity<UserToken>()
                .HasKey(ut => ut.tokenId);

            modelBuilder.Entity<Statistics>()
                .HasKey(s => s.statisticId);


            modelBuilder.Entity<User>()
               .HasMany(u=>u.chatHistory)
               .WithOne(c => c.user)
               .HasForeignKey(c => c.userId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Role>()
               .HasMany(r => r.users)
               .WithOne(u => u.role)
               .HasForeignKey(u => u.roleId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<UserToken>()
                .HasOne(ut => ut.user)
                .WithOne(u => u.userToken)
                .HasForeignKey<UserToken>(ut => ut.userId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Statistics>()
                .HasOne(s => s.user)
                .WithMany(u => u.statistics)
                .HasForeignKey(s => s.userId)
                .OnDelete(DeleteBehavior.Cascade);


            base.OnModelCreating(modelBuilder);

        }

    }
}
