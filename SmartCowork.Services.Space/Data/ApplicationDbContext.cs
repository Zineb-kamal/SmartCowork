
// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using SmartCowork.Services.Space.Models;

namespace SmartCowork.Services.Space.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Models.Space> Spaces { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Space>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Capacity).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.PricePerHour).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PricePerDay).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PricePerWeek).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PricePerMonth).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PricePerYear).HasColumnType("decimal(18,2)");
            });
        }
    }
}