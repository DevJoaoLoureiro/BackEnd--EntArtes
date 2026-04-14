using EscolaDanca.Data;
using EscolaDanca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    private readonly AppDbContext _db;

    public ConfigController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("tipo-aula")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> GetTiposAula()
    {
        var tipos = await _db.TipoAula
            .AsNoTracking()
            .OrderBy(t => t.Nome)
            .Select(t => new
            {
                t.Id,
                t.Nome,
                t.Descricao,
                t.DuracaoPadrao
            })
            .ToListAsync();

        return Ok(tipos);
    }

    // ============================
    // POST /api/config/tipo-aula
    // ============================
    [HttpPost("tipo-aula")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> CriarTipoAula([FromBody] TipoAula req)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return BadRequest("Nome é obrigatório.");

        var existe = await _db.TipoAula
            .AnyAsync(t => t.Nome == req.Nome);

        if (existe)
            return BadRequest("Já existe um tipo de aula com esse nome.");

        var tipo = new TipoAula
        {
            Nome = req.Nome.Trim(),
            Descricao = string.IsNullOrWhiteSpace(req.Descricao) ? null : req.Descricao.Trim(),
            DuracaoPadrao = req.DuracaoPadrao
        };

        _db.TipoAula.Add(tipo);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            tipo.Id,
            tipo.Nome,
            tipo.Descricao,
            tipo.DuracaoPadrao
        });
    }

    // =========================
    // CREATE ESTUDIO
    // =========================
    [HttpPost("estudio")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> CreateEstudio([FromBody] Estudio req)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return BadRequest("Nome obrigatório.");

        var exists = await _db.Estudio.AnyAsync(x => x.Nome == req.Nome);
        if (exists)
            return BadRequest("Já existe um estúdio com esse nome.");

        var estudio = new Estudio
        {
            Nome = req.Nome.Trim()
        };

        _db.Estudio.Add(estudio);
        await _db.SaveChangesAsync();

        return Ok(estudio);
    }

    // =========================
    // GET ESTUDIO
    // =========================
    [HttpGet("estudio")]
    public async Task<IActionResult> GetEstudio()
    {
        var list = await _db.Estudio
            .OrderBy(x => x.Nome)
            .Select(x => new
            {
                x.Id,
                x.Nome
            })
            .ToListAsync();

        return Ok(list);
    }
}