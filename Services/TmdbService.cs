// Services/TmdbService.cs
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CineStream.Services;

public interface ITmdbService
{
  Task<TmdbMovieListResponse> GetTrendingAsync(int page = 1);
  Task<TmdbMovieListResponse> GetPopularAsync(int page = 1);
  Task<TmdbMovieListResponse> GetTopRatedAsync(int page = 1);
  Task<TmdbMovieListResponse> GetNowPlayingAsync(int page = 1);
  Task<TmdbMovieListResponse> SearchMoviesAsync(string query, int page = 1);
  Task<TmdbMovieDetail> GetMovieDetailAsync(int movieId);
  Task<TmdbMovieListResponse> GetByGenreAsync(int genreId, int page = 1);
}

public class TmdbService : ITmdbService
{
  private readonly HttpClient _http;
  private readonly string _apiKey= "d48d448f8cbbd083266f168ec97524dc";
  private readonly string _baseUrl;

  private readonly JsonSerializerOptions _jsonOptions = new()
  {
    PropertyNameCaseInsensitive = true
  };

  public TmdbService(HttpClient http, IConfiguration config)
  {
    _http = http;
    _apiKey = config["Tmdb:ApiKey"]!;
    _baseUrl = config["Tmdb:BaseUrl"]!;
  }

  private string Url(string endpoint, string extraParams = "") =>
      $"{_baseUrl}{endpoint}?api_key={_apiKey}&language=en-US{extraParams}";

  public async Task<TmdbMovieListResponse> GetTrendingAsync(int page)
  {
    var res = await _http.GetAsync(Url("/trending/movie/week", $"&page={page}"));
    return await Deserialize<TmdbMovieListResponse>(res);
  }

  public async Task<TmdbMovieListResponse> GetPopularAsync(int page = 1)
  {
    var res = await _http.GetAsync(Url("/movie/popular", $"&page={page}"));
    return await Deserialize<TmdbMovieListResponse>(res);
  }

  public async Task<TmdbMovieListResponse> GetTopRatedAsync(int page = 1)
  {
    var res = await _http.GetAsync(Url("/movie/top_rated", $"&page={page}"));
    return await Deserialize<TmdbMovieListResponse>(res);
  }

  public async Task<TmdbMovieListResponse> GetNowPlayingAsync(int page = 1)
  {
    var res = await _http.GetAsync(Url("/movie/now_playing", $"&page={page}"));
    return await Deserialize<TmdbMovieListResponse>(res);
  }

  public async Task<TmdbMovieListResponse> SearchMoviesAsync(string query, int page = 1)
  {
    var encoded = Uri.EscapeDataString(query);
    var res = await _http.GetAsync(Url("/search/movie", $"&query={encoded}&page={page}"));
    return await Deserialize<TmdbMovieListResponse>(res);
  }

  public async Task<TmdbMovieDetail> GetMovieDetailAsync(int movieId)
  {
    var res = await _http.GetAsync(Url($"/movie/{movieId}", "&append_to_response=videos,credits"));
    return await Deserialize<TmdbMovieDetail>(res);
  }

  public async Task<TmdbMovieListResponse> GetByGenreAsync(int genreId, int page = 1)
  {
    var res = await _http.GetAsync(Url("/discover/movie", $"&with_genres={genreId}&page={page}"));
    return await Deserialize<TmdbMovieListResponse>(res);
  }

  private async Task<T> Deserialize<T>(HttpResponseMessage res)
  {
    res.EnsureSuccessStatusCode();
    var json = await res.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
  }

  //public async Task<object> GetMovieDetailAsync(int id)
  //{
  //  var url = $"{_baseUrl}movie/{id}?api_key={_apiKey}";
  //  var res = await _http.GetAsync(url);

  //  if (!res.IsSuccessStatusCode) return null;

  //  var data = await res.Content.ReadAsStringAsync();
  //  return JsonSerializer.Deserialize<object>(data);
  //}

  public async Task<object> SearchMovieAsync(string name)
  {
    var url = $"{_baseUrl}search/movie?api_key={_apiKey}&query={name}";
    var res = await _http.GetAsync(url);

    if (!res.IsSuccessStatusCode) return null;

    var data = await res.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<object>(data);
  }
}

// ──── TMDB Response Models ────

public class TmdbMovieListResponse
{
  public int Page { get; set; }
  public List<TmdbMovie> Results { get; set; } = new();
  [JsonPropertyName("total_pages")]
  public int TotalPages { get; set; }
  [JsonPropertyName("total_results")]
  public int TotalResults { get; set; }
}

public class TmdbMovie
{
  public int Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Overview { get; set; } = string.Empty;
  [JsonPropertyName("poster_path")]
  public string? PosterPath { get; set; }
  [JsonPropertyName("backdrop_path")]
  public string? BackdropPath { get; set; }
  [JsonPropertyName("vote_average")]
  public double VoteAverage { get; set; }
  [JsonPropertyName("release_date")]
  public string? ReleaseDate { get; set; }
  [JsonPropertyName("genre_ids")]
  public List<int> GenreIds { get; set; } = new();
  public bool Adult { get; set; }
  public string PosterUrl => PosterPath != null
      ? $"https://image.tmdb.org/t/p/w500{PosterPath}" : "";
  public string BackdropUrl => BackdropPath != null
      ? $"https://image.tmdb.org/t/p/w1280{BackdropPath}" : "";
}

public class TmdbMovieDetail : TmdbMovie
{
  public int Runtime { get; set; }
  public string Status { get; set; } = string.Empty;
  public string Tagline { get; set; } = string.Empty;
  public long Budget { get; set; }
  public long Revenue { get; set; }
  public List<Genre> Genres { get; set; } = new();
  public Videos? Videos { get; set; }
  public Credits? Credits { get; set; }
}

public class Genre
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
}

public class Videos
{
  public List<Video> Results { get; set; } = new();
}

public class Video
{
  public string Key { get; set; } = string.Empty;
  public string Site { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty;
  public bool Official { get; set; }
}

public class Credits
{
  public List<CastMember> Cast { get; set; } = new();
}

public class CastMember
{
  public string Name { get; set; } = string.Empty;
  public string Character { get; set; } = string.Empty;
  [JsonPropertyName("profile_path")]
  public string? ProfilePath { get; set; }
}

