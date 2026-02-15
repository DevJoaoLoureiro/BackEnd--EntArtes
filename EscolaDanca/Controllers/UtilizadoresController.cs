using EscolaDanca.Data;
using EscolaDanca.DTOs;
using EscolaDanca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaDanca.Controllers;

[ApiController]
[Route("api/utilizadores")]
public class UtilizadoresController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordService _pw;

    public UtilizadoresController(AppDbContext db, PasswordService pw)
    {
        _db = db;
        _pw = pw;
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Criar([FromBody] CriarUtilizadorRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nome) ||
            string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.Password) ||
            string.IsNullOrWhiteSpace(req.Perfil))
            return BadRequest("Preenche nome, username, password e perfil.");

        req.Username = req.Username.Trim();

        var perfilOk = new[] { "ADMIN", "SUPER_ADMIN", "PROFESSOR", "ENCARREGADO" }.Contains(req.Perfil);
        if (!perfilOk) return BadRequest("Perfil inválido.");

        var exists = await _db.Utilizadores.AnyAsync(u => u.Username == req.Username);
        if (exists) return BadRequest("Esse username já existe.");

        var user = new Models.Utilizador
        {
            Nome = req.Nome.Trim(),
            Username = req.Username,
            Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim(),
            PasswordHash = _pw.Hash(req.Password),
            Perfil = req.Perfil,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _db.Utilizadores.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { user.Id, user.Nome, user.Username, user.Email, user.Perfil, user.Ativo });
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Listar()
    {
        var utilizadores = await _db.Utilizadores
            .OrderBy(u => u.Nome)
            .Select(u => new {
                u.Id,
                u.Nome,
                u.Username,
                u.Email,
                u.Perfil,
                u.Ativo
            })
            .ToListAsync();
        return Ok(utilizadores);
    }


}
