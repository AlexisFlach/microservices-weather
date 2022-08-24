using System;
using Microsoft.EntityFrameworkCore;

namespace CloudWeather.Temperature.DataAccess
{
    public class TemperatureDbContext : DbContext
    {
        public TemperatureDbContext() { }

        public TemperatureDbContext(DbContextOptions opts) : base(opts) { }

        public DbSet<Temperature> Temperature { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            SnakeCaseIdentityTableName(modelBuilder);
        }

        private static void SnakeCaseIdentityTableName(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Temperature>(b => { b.ToTable("temperature"); });
        }
    }
}

