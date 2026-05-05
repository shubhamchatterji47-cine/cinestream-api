namespace CineStream.Models
{
  public class User
  {
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; } = false; // ✅ NEW
    public string? EmailToken { get; set; }              // ✅ NEW
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<WatchlistItem> Watchlist { get; set; } = new();
    public int FailedAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
  }
}
