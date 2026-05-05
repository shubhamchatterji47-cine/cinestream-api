namespace CineStream.Models
{
  public class WatchlistItem
  {
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TmdbMovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string PosterPath { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
  }
}
