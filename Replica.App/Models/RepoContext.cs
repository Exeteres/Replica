using System;
using Microsoft.EntityFrameworkCore;
using Replica.Core.Configuration;
using Replica.Core.Exceptions;

namespace Replica.App.Models
{
    public class RepoContext : DbContext
    {
        private static DbContextOptions<RepoContext> _options = new DbContextOptions<RepoContext>();

        public RepoContext() : base(_options)
        {
            Database.Migrate();
        }

        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMember> ChatMembers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<ChatRoute> ChatRoutes { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<KeyValue> Values { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (Program.Options == null)
                Program.LoadSettings();

            if (Program.Options.Database == null)
                throw new InvalidConfigurationException("Missing database configuration");

            if (!options.IsConfigured)
            {
                options.UseLazyLoadingProxies();
                switch (Program.Options.Database.Type)
                {
                    case "sqlite":
                        options.UseSqlite(Program.Options.Database.Connection);
                        break;
                    case "mysql":
                        options.UseMySql(Program.Options.Database.Connection);
                        break;
                    default: throw new InvalidConfigurationException("Invalid database type. Use sqlite or mysql");
                }
            }
        }
    }
}