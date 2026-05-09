using EscolaDanca.Data;
using EscolaDanca.DTOs;
using EscolaDanca.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EscolaDanca.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;
    private readonly PasswordService _pw;

    public AuthController(AppDbContext db, IConfiguration cfg, PasswordService pw)
    {
        _db = db;
        _cfg = cfg;
        _pw = pw;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        try
        {
            if (req == null)
                return BadRequest("Pedido inválido.");

            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Username e password são obrigatórios.");

            var username = req.Username.Trim();

            var user = await _db.Utilizadores
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username && u.Ativo);

            if (user == null)
                return Unauthorized("Credenciais inválidas.");

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                return StatusCode(500, "O utilizador não tem password válida guardada.");

            var passwordOk = _pw.Verify(req.Password, user.PasswordHash);
            if (!passwordOk)
                return Unauthorized("Credenciais inválidas.");

            var jwtKey = _cfg["Jwt:Key"];
            var jwtIssuer = _cfg["Jwt:Issuer"];
            var jwtAudience = _cfg["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                return StatusCode(500, "Jwt:Key não está configurado.");

            if (string.IsNullOrWhiteSpace(jwtIssuer))
                return StatusCode(500, "Jwt:Issuer não está configurado.");

            if (string.IsNullOrWhiteSpace(jwtAudience))
                return StatusCode(500, "Jwt:Audience não está configurado.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Perfil)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddHours(8);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                user = new
                {
                    id = user.Id,
                    nome = user.Nome,
                    username = user.Username,
                    email = user.Email,
                    perfil = user.Perfil,
                    ativo = user.Ativo
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message,
                inner = ex.InnerException?.Message
            });
        }
    }
}