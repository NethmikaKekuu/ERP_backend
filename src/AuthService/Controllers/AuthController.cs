using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly TokenRevocationService _revocation;

    public AuthController(IConfiguration config, TokenRevocationService revocation)
    {
        _config = config;
        _revocation = revocation;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Hardcoded for now — DB integration comes later
        if (request.Username == "admin" && request.Password == "password")
        {
            var jti = Guid.NewGuid().ToString();
            var token = GenerateJwtToken(request.Username, "ADMIN", jti);

            // Revoke any previously issued token for this user
            _revocation.SetActiveToken(request.Username, jti);

            return Ok(new { token });
        }

        return Unauthorized(new { message = "Invalid credentials" });
    }

    private string GenerateJwtToken(string username, string role, string jti)
    {
        var secret = _config["Jwt:Secret"] ?? "your_super_secret_key_here";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string Username, string Password);