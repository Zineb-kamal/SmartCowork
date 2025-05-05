// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using SmartCowork.Services.AI.Models;

namespace SmartCowork.Services.AI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<SpaceRating> SpaceRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }
    }

    // Ajout d'une classe pour les évaluations d'espaces
    public class SpaceRating
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid SpaceId { get; set; }
        public float Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}