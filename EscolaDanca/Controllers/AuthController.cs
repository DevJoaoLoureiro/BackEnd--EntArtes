using EscolaDanca.Data;
using EscolaDanca.DTOs;
using EscolaDanca.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EscolaDanca.Api.Controllers;

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


    [HttpPost("hash")]
    public IActionResult Hash([FromBody] string password)
    {
        return Ok(_pw.Hash(password));
    }



    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username e password são obrigatórios.");

        var user = await _db.Utilizadores
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == req.Username && u.Ativo);

        if (user == null) return Unauthorized("Credenciais inválidas.");

        if (!_pw.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Credenciais inválidas.");


        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Perfil)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = tokenStr,
            user = new
            {
                id = user.Id,
                nome = user.Nome,
                username = user.Username,
                perfil = user.Perfil
            }
        });
    }
}
