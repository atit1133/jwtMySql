using System;
using Microsoft.EntityFrameworkCore;
using JwtMySql.Models;



namespace JwtMySql
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        // Optional: Only if you want to configure here (not recommended for production)
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     if (!optionsBuilder.IsConfigured)
        //     {
        //         optionsBuilder.UseMySql(
        //             "server=localhost;database=jwt_db;user=root;password=yourpassword;",
        //             ServerVersion.AutoDetect("server=localhost;database=jwt_db;user=root;password=yourpassword;")
        //         );
        //     }
        // }
    }
}

