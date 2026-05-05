using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CineStream.Data;
using CineStream.Models;

namespace CineStream.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // ✅ MUST ENABLE THIS
public class WatchlistController : ControllerBase
{
  private readonly AppDbContext _db;

  public WatchlistController(AppDbContext db)
  {
    _db = db;
  }

  // ✅ Safe way to get userId
  private int? GetUserId()
  {
    var claim = User.FindFirst("Id");

    if (claim == null)
      return null;

    if (int.TryParse(claim.Value, out int userId))
      return userId;

    return null;
  }

  // GET: api/watchlist
  [HttpGet]
  public async Task<IActionResult> GetWatchlist()
  {
    var userId = GetUserId();
    if (userId == null)
      return Unauthorized();

    var items = await _db.WatchlistItems
        .Where(w => w.UserId == userId)
        .OrderByDescending(w => w.AddedAt)
        .ToListAsync();

    return Ok(items);
  }

  // POST: api/watchlist
  [HttpPost]
  public async Task<IActionResult> AddToWatchlist([FromBody] AddWatchlistDto dto)
  {
    var userId = GetUserId();
    if (userId == null)
      return Unauthorized();

    var exists = await _db.WatchlistItems
        .AnyAsync(w => w.UserId == userId && w.TmdbMovieId == dto.TmdbMovieId);

    if (exists)
      return BadRequest(new { message = "Already in watchlist." });

    var item = new WatchlistItem
    {
      UserId = userId.Value,
      TmdbMovieId = dto.TmdbMovieId,
      MovieTitle = dto.MovieTitle,
      PosterPath = dto.PosterPath,
      AddedAt = DateTime.UtcNow // ✅ important
    };

    _db.WatchlistItems.Add(item);
    await _db.SaveChangesAsync();

    return Ok(item);
  }

  // DELETE: api/watchlist/{tmdbMovieId}
  [HttpDelete("{tmdbMovieId:int}")]
  public async Task<IActionResult> RemoveFromWatchlist(int tmdbMovieId)
  {
    var userId = GetUserId();
    if (userId == null)
      return Unauthorized();

    var item = await _db.WatchlistItems
        .FirstOrDefaultAsync(w => w.UserId == userId && w.TmdbMovieId == tmdbMovieId);

    if (item == null)
      return NotFound(new { message = "Item not found." });

    _db.WatchlistItems.Remove(item);
    await _db.SaveChangesAsync();

    return Ok(new { message = "Removed from watchlist." });
  }
}

// DTO
public record AddWatchlistDto(
    int TmdbMovieId,
    string MovieTitle,
    string PosterPath
);
