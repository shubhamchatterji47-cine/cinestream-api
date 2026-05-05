//// Services/TokenService.cs
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using CineStream.Models;

//namespace CineStream.Services;

//public interface ITokenService
//{
//  (string Token, DateTime ExpiresAt) GenerateToken(User user);
//}

//public class TokenService : ITokenService
//{
//  private readonly IConfiguration _config;

//  public TokenService(IConfiguration config)
//  {
//    _config = config;
//  }

//  public (string Token, DateTime ExpiresAt) GenerateToken(User user)
//  {
//    var key = new SymmetricSecurityKey(
//        Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

//    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//    var expiryHours = int.Parse(_config["Jwt:ExpiryHours"] ?? "24");
//    var expiresAt = DateTime.UtcNow.AddHours(expiryHours);

//    // Claims embedded in the JWT
//    var claims = new[]
//    {
//            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
//            new Claim(JwtRegisteredClaimNames.Email, user.Email),
//            new Claim(JwtRegisteredClaimNames.Name, user.FullName),
//            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//        };

//    var token = new JwtSecurityToken(
//        issuer: _config["Jwt:Issuer"],
//        audience: _config["Jwt:Audience"],
//        claims: claims,
//        expires: expiresAt,
//        signingCredentials: credentials
//    );

//    return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
//  }
//}
