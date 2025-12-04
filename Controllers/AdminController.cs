using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Table_Reservation.Models;
using System.Security.Claims;
using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;

[ApiController]
[Route("api/[controller]")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration; // Ajout pour récupérer les paramètres JWT


    public AdminController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;

    }
    
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel loginData)
    {
        if (loginData == null || string.IsNullOrEmpty(loginData.Username) || string.IsNullOrEmpty(loginData.Password))
        {
            return BadRequest(new { message = "Nom d'utilisateur et mot de passe requis." });
        }

        var admin = _context.Administrators.SingleOrDefault(a => a.Username == loginData.Username);

        if (admin == null)
        {
            return Unauthorized(new { message = "Nom d'utilisateur ou mot de passe incorrect." });
        }

        bool passwordValid = false;
        try
        {
            passwordValid = BCryptNet.Verify(loginData.Password, admin.Password);
        }
        catch
        {
            passwordValid = false;
        }
        // Fallback clair si le mot de passe en base n'est pas encore haché
        if (!passwordValid && loginData.Password == admin.Password)
        {
            passwordValid = true;
            try
            {
                admin.Password = BCryptNet.HashPassword(loginData.Password);
                await _context.SaveChangesAsync();
            }
            catch { }
        }

        if (!passwordValid)
        {
            return Unauthorized(new { message = "Nom d'utilisateur ou mot de passe incorrect." });
        }

        var secretKey = _configuration["Jwt:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            return StatusCode(500, new { message = "Clé secrète JWT non configurée." });
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.Name, admin.Username),
        new Claim(ClaimTypes.Role, "Admin")
    };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(2500),
            signingCredentials: credentials
        );

        if (token == null)
        {
            return StatusCode(500, new { message = "Erreur lors de la génération du token." });
        }

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // Déposer le token dans un cookie HttpOnly sécurisé
        Response.Cookies.Append(
            "access_token",
            tokenString,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddSeconds(30)
            }
        );

        return Ok(new { token = tokenString });
    }


    [HttpPost("Logout")]
    public IActionResult Logout()
    {
        // Supprimer le cookie de token JWT
        Response.Cookies.Delete("access_token");
        return Ok(new { message = "Déconnecté avec succès." });

    }
    [HttpPost("VerifyToken")]
    public IActionResult VerifyToken()
    {
        // Lire d'abord le cookie HttpOnly, sinon l'en-tête Authorization
        var token = Request.Cookies["access_token"];
        if (string.IsNullOrEmpty(token))
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }
        }
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new { message = "Aucun token fourni." });
        }

        try
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            tokenHandler.ValidateToken(token, validationParams, out _);
            return Ok(new { message = "Token valide." });
        }
        catch (SecurityTokenExpiredException)
        {
            return Unauthorized(new { message = "Token expiré." });
        }
        catch (Exception)
        {
            return Unauthorized(new { message = "Token invalide." });
        }
    }

}
