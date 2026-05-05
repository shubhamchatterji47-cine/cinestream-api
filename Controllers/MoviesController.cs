// Controllers/MoviesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CineStream.Services;

namespace CineStream.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // All movie endpoints require a valid JWT
public class MoviesController : ControllerBase
{
  private readonly ITmdbService _tmdb;

  public MoviesController(ITmdbService tmdb)
  {
    _tmdb = tmdb;
  }

  // GET /api/movies/trending?page=1
  [HttpGet("trending")]
  public async Task<IActionResult> Trending(int page = 1)
  {
    var result = await _tmdb.GetTrendingAsync(page);
    return Ok(result);
  }

  // GET /api/movies/popular?page=1
  [HttpGet("popular")]
  public async Task<IActionResult> Popular(int page = 1)
  {
    var result = await _tmdb.GetPopularAsync(page);
    return Ok(result);
  }

  // GET /api/movies/top-rated?page=1
  [HttpGet("top-rated")]
  public async Task<IActionResult> TopRated(int page = 1)

  {
    var result = await _tmdb.GetTopRatedAsync(page);
    return Ok(result);
  }

  // GET /api/movies/now-playing?page=1
  [HttpGet("now-playing")]
  public async Task<IActionResult> NowPlaying(int page = 1)
  {
    var result = await _tmdb.GetNowPlayingAsync(page);
    return Ok(result);
  }

  // GET: /api/movies/search?name=avatar
  [HttpGet("search")]
  public async Task<IActionResult> SearchByName([FromQuery] string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return BadRequest(new { message = "Movie name is required." });

    var result = await _tmdb.SearchMoviesAsync(name);

    return Ok(result);
  }

  // GET /api/movies/{id}
  [HttpGet("{id:int}")]
  public async Task<IActionResult> Detail(int id)
  {
    var result = await _tmdb.GetMovieDetailAsync(id);
    return Ok(result);
  }

  //// GET /api/movies/genre/{genreId}?page=1
  //[HttpGet("genre/{genreId:int}")]
  //public async Task<IActionResult> ByGenre(int genreId, [FromQuery] int page = 1)
  //{
  //  var result = await _tmdb.GetByGenreAsync(genreId, page);
  //  return Ok(result);
  //}
}
