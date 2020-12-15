using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSign.Models;

namespace VSign.DbContexts
{
    public class AuthContext : DbContext
    {
        public AuthContext(DbContextOptions<AuthContext> options) : base(options) { }
        public AuthContext() { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<App> Apps { get; set; }
        public DbSet<UserRoleMap> UserRoleMaps { get; set; }
        public DbSet<RoleAppMap> RoleAppMaps { get; set; }
        public DbSet<UserLoginHistory> UserLoginHistory { get; set; }
        public DbSet<TokenHistory> TokenHistories { get; set; }
        public DbSet<SessionMaster> SessionMasters { get; set; }
        public DbSet<AppUsage> AppUsages { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoleAppMap>(
            build =>
            {
                build.HasKey(t => new { t.RoleID, t.AppID });
                //build.HasOne(t => t.RoleID).WithOne().HasForeignKey<Role>(qe => qe.RoleID);
                //build.HasOne(t => t.AppID).WithOne().HasForeignKey<App>(qe => qe.AppID);
            });
            modelBuilder.Entity<UserRoleMap>(
            build =>
            {
                build.HasKey(t => new { t.UserID, t.RoleID });
                //build.HasOne(t => t.RoleID).WithOne().HasForeignKey<Role>(qe => qe.RoleID);
                //build.HasOne(t => t.AppID).WithOne().HasForeignKey<App>(qe => qe.AppID);
            });
            modelBuilder.Entity<Client>().HasData(BuildClientsList());
            modelBuilder.Entity<AppUsage>().HasIndex(table => new { table.UserID, table.AppName });
        }
        private Client[] BuildClientsList()
        {
            List<Client> ClientsList = new List<Client>
            {
                new Client
                { Id = "ngAuthApp",
                    Secret= Helper.GetHash("abc@123"),
                    Name="AngularJS front-end Application",
                    ApplicationType =  Models.ApplicationTypes.JavaScript,
                    Active = true,
                    RefreshTokenLifeTime = 7200,
                    AllowedOrigin = "*"
                },
                new Client
                { Id = "consoleApp",
                    Secret=Helper.GetHash("123@abc"),
                    Name="Console Application",
                    ApplicationType =Models.ApplicationTypes.NativeConfidential,
                    Active = true,
                    RefreshTokenLifeTime = 14400,
                    AllowedOrigin = "*"
                }
            };

            return ClientsList.ToArray();
        }
    
}
}
