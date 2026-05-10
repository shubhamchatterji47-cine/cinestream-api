//using CineStream.Data;
//using CineStream.Services;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.RateLimiting;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Options;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using System;
//using System.Text;
//using Microsoft.AspNetCore.RateLimiting;
//using System.Threading.RateLimiting;

//var builder = WebApplication.CreateBuilder(args);

//// Add services
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// Database (SQLite for simplicity — swap to SQL Server in prod)
//builder.Services.AddDbContext<AppDbContext>(options =>
//    //options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//    options.UseSqlite("Data Source=cinestream.db"));

//// JWT Authentication
////var jwtKey = builder.Configuration["Jwt:Key"]!;
//builder.Services.AddAuthentication(
//  options =>
//  {
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//  })
//    .AddJwtBearer(options =>
//    {
//      options.TokenValidationParameters = new TokenValidationParameters
//      {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//      };
//    });

//builder.Services.AddAuthorization();

//// CORS for Angular frontend
//builder.Services.AddCors(options =>
//{
//  options.AddPolicy("AllowAngular", policy => policy.WithOrigins("http://localhost:4200", "https://your-app.vercel.app")
//            .AllowAnyHeader()
//            .AllowAnyMethod());
//});
//builder.Services.AddSwaggerGen(options =>
//{
//  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//  {
//    Name = "Authorization",
//    Type = SecuritySchemeType.Http,
//    Scheme = "bearer",
//    BearerFormat = "JWT",
//    In = ParameterLocation.Header,
//    Description = "Enter JWT Token like: Bearer {your token}"
//  });

//  options.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            new string[] {}
//        }
//    });
//});

//// Custom services
//builder.Services.AddHttpClient<ITmdbService, TmdbService>(client =>
//{
//  client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
//  client.DefaultRequestHeaders.Add("Accept", "application/json");
//  client.DefaultRequestHeaders.Add("User-Agent", "CineStreamApp");
//})
//.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//{
//  AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
//});
////builder.Services.AddScoped<IAuthService, AuthService>();
////builder.Services.AddScoped<ITokenService, TokenService>();
//builder.Services.AddScoped<EmailService>();



//builder.Services.AddRateLimiter(options =>
//{
//  options.AddFixedWindowLimiter("authPolicy", opt =>
//  {
//    opt.PermitLimit = 5;              // 🔥 max 5 requests
//    opt.Window = TimeSpan.FromMinutes(1); // per 1 minute
//    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//    opt.QueueLimit = 2;
//  });

//  options.RejectionStatusCode = 429; // Too Many Requests
//});


//var app = builder.Build();

//// Middleware pipeline
//if (app.Environment.IsDevelopment())
//{
//  app.UseSwagger();
//  app.UseSwaggerUI();
//}

//app.UseCors("AllowAngular");
//app.UseRateLimiter();
//app.UseAuthentication();
//app.UseAuthorization();
//app.UseHttpsRedirection();
//app.MapControllers();

//// Auto-create DB on startup
//using (var scope = app.Services.CreateScope())
//{
//  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//  db.Database.EnsureCreated();
//}


//app.Run();
using CineStream.Data;
using CineStream.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ PostgreSQL on Railway, SQLite locally
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

// builder.Services.AddDbContext<AppDbContext>(options =>
// {
//   if (databaseUrl != null && databaseUrl.StartsWith("postgresql://"))
//   {
//     var uri = new Uri(databaseUrl);
//     var userInfo = uri.UserInfo.Split(':');
//     var connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
//     options.UseNpgsql(connectionString);
//   }
//   else
//   {
//     // Local development uses SQLite
//     options.UseSqlite("Data Source=cinestream.db");
//   }
// });
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");

    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite("Data Source=cinestream.db");
    }
});

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
  };
});

builder.Services.AddAuthorization();

//builder.Services.AddCors(options =>
//{
//  options.AddPolicy("AllowAngular", policy =>
//      policy.AllowAnyOrigin()
//      .AllowAnyHeader()
//      .AllowAnyMethod());
//});
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAngular", policy =>
      policy.WithOrigins(
          "http://localhost:4200",
          "https://localhost:4200",
          "https://cinestream-frontend-nine.vercel.app"
      )
      .AllowAnyHeader()
      .AllowAnyMethod());
});

builder.Services.AddSwaggerGen(options =>
{
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Enter JWT Token like: Bearer {your token}"
  });
  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
      },
      new string[] {}
    }
  });
});

builder.Services.AddHttpClient<ITmdbService, TmdbService>(client =>
{
  client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
  client.DefaultRequestHeaders.Add("Accept", "application/json");
  client.DefaultRequestHeaders.Add("User-Agent", "CineStreamApp");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
  AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
});

builder.Services.AddScoped<EmailService>();

builder.Services.AddRateLimiter(options =>
{
  options.AddFixedWindowLimiter("authPolicy", opt =>
  {
    opt.PermitLimit = 5;
    opt.Window = TimeSpan.FromMinutes(1);
    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    opt.QueueLimit = 2;
  });
  options.RejectionStatusCode = 429;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngular");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  // db.Database.EnsureCreated();
  db.Database.Migrate();
}

app.Run();
