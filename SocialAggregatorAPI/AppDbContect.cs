using Microsoft.EntityFrameworkCore;
using SocialAggregatorAPI.Models;

namespace SocialAggregatorAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<NewsArticle>().ToTable("NewsArticles");
        }
    }
}
