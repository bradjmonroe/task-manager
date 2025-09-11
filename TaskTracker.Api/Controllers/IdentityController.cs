using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Contracts;
using TaskTracker.Data;
using TaskTracker.Data.Models;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class IdentityController : ControllerBase
{
    private readonly TasksDb _db;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _signingKey;

    public IdentityController(TasksDb db, IPasswordHasher<AppUser> hasher, 
                          IConfiguration cfg, SymmetricSecurityKey signingKey)
    {
        _db = db;
        _hasher = hasher;
        _signingKey = signingKey; 
        _issuer = cfg["Jwt:Issuer"] ?? throw new InvalidOperationException("Missing Jwt:Issuer");
        _audience = cfg["Jwt:Audience"] ?? throw new InvalidOperationException("Missing Jwt:Audience");
    }

    private string GenerateJwt(AppUser user)
    {
        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var token = new JwtSecurityToken(_issuer, _audience, claims,
            expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Email and password required.");

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict("Email already registered.");

        var user = new AppUser { Email = email };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var jwt = GenerateJwt(user);
        return Ok(new { token = jwt, userId = user.Id, email = user.Email });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var email = (dto.Email ?? "").Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return Unauthorized();

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed) return Unauthorized();

        var jwt = GenerateJwt(user);
        return Ok(new { token = jwt, userId = user.Id, email = user.Email });
    }
}
