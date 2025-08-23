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

        if (admin == null || loginData.Password != admin.Password)
        {
            return Unauthorized(new { message = "Nom d'utilisateur ou mot de passe incorrect." });
        }

        // 🔹 Vérifier si la clé est bien récupérée
        Console.WriteLine($"🔹 SecretKey récupérée: {_configuration["Jwt:SecretKey"]}");
        Console.WriteLine($"🔹 Issuer récupéré: {_configuration["Jwt:Issuer"]}");
        Console.WriteLine($"🔹 Audience récupérée: {_configuration["Jwt:Audience"]}");

        var secretKey = _configuration["Jwt:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            return StatusCode(500, new { message = "🚨 Clé secrète JWT non configurée." });
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
            expires: DateTime.UtcNow.AddSeconds(50),
            signingCredentials: credentials
        );

        if (token == null)
        {
            Console.WriteLine("🚨 Erreur: Token JWT non généré !");
            return StatusCode(500, new { message = "Erreur lors de la génération du token." });
        }

        Console.WriteLine("✅ Token généré avec succès !");
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        Console.WriteLine($"🔹 Token JWT : {tokenString}");

        return Ok(new { token = tokenString });
    }


    [HttpPost("Logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Déconnecté avec succès." });

    }
    [HttpPost("VerifyToken")]
    public IActionResult VerifyToken()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new { message = "Aucun token fourni." });
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

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
