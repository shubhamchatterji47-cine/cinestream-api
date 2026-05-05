using CineStream.Models;
using Microsoft.EntityFrameworkCore;

namespace CineStream.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<WatchlistItem> WatchlistItems { get; set; }

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //  // Unique email constraint
    //  modelBuilder.Entity<User>()
    //      .HasIndex(u => u.Email)
    //      .IsUnique();

    //  // One user has many watchlist items
    //  modelBuilder.Entity<WatchlistItem>()
    //      .HasOne(w => w.User)
    //      .WithMany(u => u.Watchlist)
    //      .HasForeignKey(w => w.UserId);
    //}
  }
}
