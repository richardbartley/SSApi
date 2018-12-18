using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SSApi.EFCore
{
    /// <summary>
    /// SQLite context to support Unit Testing and Two migration sets
    /// See:  https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers
    /// </summary>
    public class SqliteContext : DataContext, IDataContext
    {
        //Add-Migrations tooling looks at the context to see where to output
        private string _connectionString = "Data Source=C:\\TestDatabases\\MBData.db";

        /// <summary>
        /// Needed to support migrations
        /// </summary>
        public SqliteContext() { }

        /// <summary>
        /// Allow our test project to supply connectionString with location of test database
        /// </summary>
        /// <param name="connectionString"></param>
        public SqliteContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(_connectionString ?? "Data Source=C:\\TestDatabases\\MBData.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //{
        //    if (!options.IsConfigured)
        //    {
        //        options.UseSqlite("Data Source=MBData.db");

        //        IConfigurationRoot configuration = new ConfigurationBuilder()
        //           .SetBasePath(Directory.GetCurrentDirectory())
        //           .AddJsonFile("appsettings.json")
        //           .Build();
        //        var connectionString = configuration.GetConnectionString("DefaultConnection");
        //        options.UseSqlite(connectionString);
        //    }
        //}
    }
}
