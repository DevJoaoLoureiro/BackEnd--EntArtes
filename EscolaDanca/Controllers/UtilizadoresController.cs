using EscolaDanca.Data;
using EscolaDanca.DTOs;
using EscolaDanca.Models;
using EscolaDanca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EscolaDanca.Controllers;

[ApiController]
[Route("api/utilizadores")]
public class UtilizadoresController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordService _pw;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _cfg;

    public UtilizadoresController(
        AppDbContext db,
        PasswordService pw,
        IEmailService emailService,
        IConfiguration cfg)
    {
        _db = db;
        _pw = pw;
        _emailService = emailService;
        _cfg = cfg;
    }

    [HttpPost("convite")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> EnviarConvite([FromBody] CriarConviteRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest("Email é obrigatório.");

        if (string.IsNullOrWhiteSpace(req.Perfil))
            return BadRequest("Perfil é obrigatório.");

        req.Perfil = req.Perfil.Trim().ToUpper();
        var perfilOk = new[] { "ADMIN", "SUPER_ADMIN", "PROFESSOR", "ENCARREGADO" }.Contains(req.Perfil);
        if (!perfilOk)
            return BadRequest("Perfil inválido.");

        var email = req.Email.Trim().ToLower();

        var emailJaExiste = await _db.Utilizadores
            .AnyAsync(u => u.Email != null && u.Email.ToLower() == email);

        if (emailJaExiste)
            return BadRequest("Já existe um utilizador com esse email.");

        var convitePendente = await _db.ConvitesUtilizador
            .AnyAsync(c => c.Email.ToLower() == email && !c.Usado && c.ExpiraEm > DateTime.UtcNow);

        if (convitePendente)
            return BadRequest("Já existe um convite pendente para esse email.");

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int criadoPor = int.Parse(userIdClaim!);

        var convite = new ConviteUtilizador
        {
            Email = email,
            Perfil = req.Perfil,
            Token = token,
            ExpiraEm = DateTime.UtcNow.AddHours(24),
            Usado = false,
            CriadoEm = DateTime.UtcNow,
            CriadoPorUtilizadorId = criadoPor
        };

        _db.ConvitesUtilizador.Add(convite);
        await _db.SaveChangesAsync();

        var frontendBase = _cfg["Frontend:BaseUrl"];
        var link = $"{frontendBase}/registo.html?token={token}";

        var assunto = "Convite para criar conta - Escola de Dança";
        var body = $@"Olá,

Recebeu um convite para criar a sua conta na plataforma da Escola de Dança.

Clique no link abaixo para concluir o registo:
{link}

Este link expira em 24 horas.

Se não estava à espera deste email, ignore esta mensagem.";

        await _emailService.SendAsync(email, assunto, body);

        return Ok(new { message = "Convite enviado com sucesso." });
    }

    [HttpGet("convite/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidarConvite(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Token inválido.");

        var convite = await _db.ConvitesUtilizador
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Token == token);

        if (convite == null)
            return NotFound("Convite inválido.");

        if (convite.Usado)
            return BadRequest("Convite já foi utilizado.");

        if (convite.ExpiraEm < DateTime.UtcNow)
            return BadRequest("Convite expirado.");

        return Ok(new
        {
            email = convite.Email,
            perfil = convite.Perfil
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Remover(int id)
    {
        var user = await _db.Utilizadores.FindAsync(id);

        if (user == null)
            return NotFound("Utilizador não encontrado.");

        //  proteção opcional: não apagar o próprio utilizador
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && int.Parse(userIdClaim) == id)
            return BadRequest("Não pode eliminar o próprio utilizador.");

        _db.Utilizadores.Remove(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Utilizador removido com sucesso." });
    }

    [HttpPost("registo-por-convite")]
    [AllowAnonymous]
    public async Task<IActionResult> RegistoPorConvite([FromBody] RegistoPorConviteRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Token) ||
            string.IsNullOrWhiteSpace(req.Nome) ||
            string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest("Dados obrigatórios em falta.");
        }

        var convite = await _db.ConvitesUtilizador
            .FirstOrDefaultAsync(c => c.Token == req.Token);

        if (convite == null)
            return BadRequest("Convite inválido.");

        if (convite.Usado)
            return BadRequest("Convite já utilizado.");

        if (convite.ExpiraEm < DateTime.UtcNow)
            return BadRequest("Convite expirado.");

        var username = req.Username.Trim();

        var usernameExiste = await _db.Utilizadores
            .AnyAsync(u => u.Username == username);

        if (usernameExiste)
            return BadRequest("Username já existe.");

        var emailExiste = await _db.Utilizadores
            .AnyAsync(u => u.Email != null && u.Email.ToLower() == convite.Email.ToLower());

        if (emailExiste)
            return BadRequest("Já existe uma conta com este email.");

        var user = new Utilizador
        {
            Nome = req.Nome.Trim(),
            Username = username,
            Email = convite.Email,
            PasswordHash = _pw.Hash(req.Password),
            Perfil = convite.Perfil,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _db.Utilizadores.Add(user);

        convite.Usado = true;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Conta criada com sucesso." });
    }

    [HttpGet("professores")]
    [Authorize]
    public async Task<IActionResult> ListarProfessores()
    {
        var profs = await _db.Utilizadores
            .AsNoTracking()
            .Where(u => u.Ativo && u.Perfil == "PROFESSOR")
            .OrderBy(u => u.Nome)
            .Select(u => new { u.Id, u.Nome })
            .ToListAsync();

        return Ok(profs);
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Listar()
    {
        var utilizadores = await _db.Utilizadores
            .OrderBy(u => u.Nome)
            .Select(u => new
            {
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