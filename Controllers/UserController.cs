//using CineStream.Data;
//using CineStream.DTOs;
//using CineStream.Models;
//using CineStream.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.RateLimiting;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace CineStream.Controllers
//{
//  [Route("api/[controller]")]
//  [ApiController]
//  public class UserController : ControllerBase
//  {
//    private readonly AppDbContext appContext;
//    private readonly EmailService _emailService;
//    private readonly IConfiguration configuration;
//    public UserController(AppDbContext appContext,IConfiguration configuration, EmailService emailService)
//    {
//      this.appContext = appContext;
//      this.configuration = configuration;
//      _emailService = emailService;
//    }
//    [EnableRateLimiting("authPolicy")]
//    [HttpPost("register")]
//    public async Task<IActionResult> Registration(UserDto userDto)
//    {
//      if (!ModelState.IsValid)
//        return BadRequest(ModelState);

//      var existingUser = appContext.Users
//          .FirstOrDefault(x => x.Email == userDto.Email);

//      if (existingUser != null)
//        return BadRequest("User already exists");

//      // 🔥 Generate email token
//      var token = Guid.NewGuid().ToString();

//      var user = new User
//      {
//        FullName = userDto.FullName,
//        Email = userDto.Email,
//        PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.PasswordHash),
//        EmailToken = token,
//        IsEmailConfirmed = false
//      };

//      appContext.Users.Add(user);
//      await appContext.SaveChangesAsync();

//      // 🔥 Send email
//      var confirmLink = $"https://localhost:7190/api/user/confirm-email?token={token}";

//      await _emailService.SendEmail(user.Email, confirmLink);

//      return Ok("Registered successfully. Please check your email.");
//    }

//    [EnableRateLimiting("authPolicy")]
//    [HttpPost("login")]
//    public IActionResult Login(LoginDto loginDto)
//    {
//      // 🔥 STEP 1: Find user by email ONLY
//      var user = appContext.Users.FirstOrDefault(x => x.Email == loginDto.Email);

//      if (user == null)
//        return Unauthorized("Invalid email or password");

//      if (!user.IsEmailConfirmed)
//      {
//        return BadRequest("Please confirm your email first.");
//      }
//      // 🔒 STEP 2: Check if user is locked
//      if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
//      {
//        return BadRequest("Account locked. Try again after 1 hour.");
//      }

//      // ❌ STEP 3: Wrong password
//      //if (user.PasswordHash != loginDto.PasswordHash)
//      if (!BCrypt.Net.BCrypt.Verify(loginDto.PasswordHash, user.PasswordHash))
//      {
//        user.FailedAttempts++;

//        // 🔥 Lock after 5 attempts
//        if (user.FailedAttempts >= 5)
//        {
//          user.LockoutEnd = DateTime.UtcNow.AddHours(1);
//        }

//        appContext.SaveChanges();

//        return Unauthorized($"Invalid password. Attempts: {user.FailedAttempts}");
//      }

//      // ✅ STEP 4: Success → reset attempts
//      user.FailedAttempts = 0;
//      user.LockoutEnd = null;
//      appContext.SaveChanges();

//      // 🔑 STEP 5: Generate JWT
//      var claims = new[]
//      {
//        new Claim(JwtRegisteredClaimNames.Sub, configuration["Jwt:Subject"]),
//        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//        new Claim("Id", user.Id.ToString()),
//        new Claim("Email", user.Email)
//    };

//      var key = new SymmetricSecurityKey(
//          Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));

//      var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//      var token = new JwtSecurityToken(
//          configuration["Jwt:Issuer"],
//          configuration["Jwt:Audience"],
//          claims,
//          expires: DateTime.UtcNow.AddHours(24),
//          signingCredentials: signIn
//      );

//      string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

//      return Ok(new
//      {
//        token = tokenValue,
//        user = user
//      });
//    }

//    [Authorize]
//    [HttpGet("GetUsers")]
//    public IActionResult GetUser()
//    {
//      return Ok(appContext.Users.ToList());
//    }
//    [Authorize]
//    [HttpGet("GetUser")]
//    public IActionResult GetUser(int id)
//    {
//      return Ok(appContext.Users.FirstOrDefault(x=>x.Id ==id));
//      if (User != null)

//        return Ok(User);
//      else
//        return NoContent();
//    }
//    [HttpGet("confirm-email")]
//    public async Task<IActionResult> ConfirmEmail(string token)
//    {
//      var user = appContext.Users
//          .FirstOrDefault(x => x.EmailToken == token);

//      if (user == null)
//        return BadRequest("Invalid token");

//      user.IsEmailConfirmed = true;
//      user.EmailToken = null;

//      await appContext.SaveChangesAsync();

//      return Ok("Email confirmed successfully");
//      // 🔥 Redirect to Angular login page
//      //return Redirect("http://localhost:4200/login");
//    }

//  }
//}
using CineStream.Data;
using CineStream.DTOs;
using CineStream.Models;
using CineStream.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CineStream.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly AppDbContext appContext;
    private readonly EmailService _emailService;
    private readonly IConfiguration configuration;

    public UserController(AppDbContext appContext, IConfiguration configuration, EmailService emailService)
    {
      this.appContext = appContext;
      this.configuration = configuration;
      _emailService = emailService;
    }

    [EnableRateLimiting("authPolicy")]
    [HttpPost("register")]
    public async Task<IActionResult> Registration(UserDto userDto)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var existingUser = appContext.Users.FirstOrDefault(x => x.Email == userDto.Email);
      if (existingUser != null)
        return BadRequest("User already exists");

      var token = Guid.NewGuid().ToString();

      var user = new User
      {
        FullName = userDto.FullName,
        Email = userDto.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.PasswordHash),
        EmailToken = token,
        IsEmailConfirmed = false
      };

      appContext.Users.Add(user);
      await appContext.SaveChangesAsync();

      var confirmLink = $"https://cinestream-api-production.up.railway.app/api/user/confirm-email?token={token}";

      // ✅ Try sending email but don't crash if it fails
      try
      {
        await _emailService.SendEmail(user.Email, confirmLink);
        return Ok("Registered successfully. Please check your email to confirm your account.");
      }
      catch (Exception ex)
      {
        // Log the error but still return success
        Console.WriteLine($"EMAIL ERROR: {ex.Message}");
        // ✅ Auto-confirm if email fails (temporary fix)
        user.IsEmailConfirmed = true;
        user.EmailToken = null;
        await appContext.SaveChangesAsync();
        return Ok("Registered successfully. You can login now.");
      }
    }

    [EnableRateLimiting("authPolicy")]
    [HttpPost("login")]
    public IActionResult Login(LoginDto loginDto)
    {
      var user = appContext.Users.FirstOrDefault(x => x.Email == loginDto.Email);
      if (user == null)
        return Unauthorized("Invalid email or password");

      if (!user.IsEmailConfirmed)
        return BadRequest("Please confirm your email first. Check your inbox.");

      if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
        return BadRequest("Account locked. Try again after 1 hour.");

      if (!BCrypt.Net.BCrypt.Verify(loginDto.PasswordHash, user.PasswordHash))
      {
        user.FailedAttempts++;
        if (user.FailedAttempts >= 5)
          user.LockoutEnd = DateTime.UtcNow.AddHours(1);
        appContext.SaveChanges();
        return Unauthorized($"Invalid password. Attempts: {user.FailedAttempts}");
      }

      user.FailedAttempts = 0;
      user.LockoutEnd = null;
      appContext.SaveChanges();

      var claims = new[]
      {
        new Claim(JwtRegisteredClaimNames.Sub, configuration["Jwt:Subject"] ?? "JwtSubject"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("Id", user.Id.ToString()),
        new Claim("Email", user.Email)
      };

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
      var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
      var jwtToken = new JwtSecurityToken(
          configuration["Jwt:Issuer"],
          configuration["Jwt:Audience"],
          claims,
          expires: DateTime.UtcNow.AddHours(24),
          signingCredentials: signIn
      );

      string tokenValue = new JwtSecurityTokenHandler().WriteToken(jwtToken);
      return Ok(new { token = tokenValue, user = user });
    }

    [Authorize]
    [HttpGet("GetUsers")]
    public IActionResult GetUsers()
    {
      return Ok(appContext.Users.ToList());
    }

    [Authorize]
    [HttpGet("GetUser")]
    public IActionResult GetUser(int id)
    {
      var user = appContext.Users.FirstOrDefault(x => x.Id == id);
      if (user != null)
        return Ok(user);
      else
        return NoContent();
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
      var user = appContext.Users.FirstOrDefault(x => x.EmailToken == token);
      if (user == null)
        return BadRequest("Invalid or expired token.");

      user.IsEmailConfirmed = true;
      user.EmailToken = null;
      await appContext.SaveChangesAsync();

      return Redirect("https://cinestream-frontend-nine.vercel.app/login");
    }
  }
}
