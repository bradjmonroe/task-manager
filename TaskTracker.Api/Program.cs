using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TaskTracker.Data;
using TaskTracker.Data.Models;

var builder = WebApplication.CreateBuilder(args);

// EF Core
var cs = builder.Configuration.GetConnectionString("Default") ?? "Data Source=tasktracker.db";
builder.Services.AddDbContext<TasksDb>(opt =>
    opt.UseSqlite(cs, b => b.MigrationsAssembly("TaskTracker.Data")));

// JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key");
var issuer = jwtSection.GetValue<string>("Issuer");
var audience = jwtSection.GetValue<string>("Audience");
if (string.IsNullOrWhiteSpace(jwtKey) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
    throw new InvalidOperationException("Missing Jwt:Key/Issuer/Audience");

var keyRaw = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("Missing Jwt:Key");

byte[] keyBytes;
try { keyBytes = Convert.FromBase64String(keyRaw); }   // Base64
catch { keyBytes = Encoding.UTF8.GetBytes(keyRaw); }   // fallback to UTF8

if (keyBytes.Length < 32)
    throw new InvalidOperationException($"Jwt:Key too short ({keyBytes.Length} bytes). Require >= 32 bytes.");

var signingKey = new SymmetricSecurityKey(keyBytes);

builder.Services.AddSingleton(signingKey);

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddAuthorization();

// DI
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

// Built-in OpenAPI
builder.Services.AddOpenApi();

// CORS
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("web", p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod());
});


// Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TasksDb>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    // OpenAPI JSON at /openapi/v1.json
    app.MapOpenApi();

    // Scalar UI at /scalar
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TaskTracker API");
        // options.WithDarkMode(true);
        // options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        // To prefill auth (only if your OpenAPI defines a Bearer scheme named "BearerAuth"):
        // options.AddPreferredSecuritySchemes("BearerAuth")
        //        .AddHttpAuthentication("BearerAuth", a => a.Token = "paste-a-dev-token-here");
    });

    app.UseCors("web");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
