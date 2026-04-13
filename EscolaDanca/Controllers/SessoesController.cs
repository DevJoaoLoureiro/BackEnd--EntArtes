using EscolaDanca.Data;
using EscolaDanca.DTOs;
using EscolaDanca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/sessoes")]
public class SessoesController : ControllerBase
{
    private readonly AppDbContext _db;
    public SessoesController(AppDbContext db) => _db = db;

    // ===============================
    // GET /api/sessoes
    // ===============================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sessoes = await _db.SessoesAula
            .OrderByDescending(s => s.Inicio)
            .Select(s => new
            {
                s.Id,
                s.Titulo,
                s.Descricao,
                s.Inicio,
                s.Fim
            })
            .ToListAsync();

        return Ok(sessoes);
    }

    // ===============================
    // GET /api/sessoes/{id}
    // ===============================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sessao = await _db.SessoesAula.FindAsync(id);
        if (sessao == null) return NotFound();

        return Ok(sessao);
    }

    // ===============================
    // POST /api/sessoes
    // ===============================
    [HttpPost]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> Create([FromBody] CreateSessao req)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var sessao = new SessaoAula
        {
            Titulo = req.Titulo,
            Descricao = req.Descricao,
            Inicio = req.DataInicio,
            Fim = req.DataFim,
            ProfessorUtilizadorId = utilizadorId
        };

        _db.SessoesAula.Add(sessao);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = sessao.Id }, sessao);
    }

    // ===============================
    // POST /api/sessoes/{id}/confirmar
    // ===============================
    [HttpPost("{id}/confirmar")]
    [Authorize]
    public async Task<IActionResult> ConfirmarPresenca(int id, [FromBody] ConfirmacaoPresenca req)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var confirmacao = await _db.ConfirmacaoPresenca
            .FirstOrDefaultAsync(c => c.SessaoAulaId == id && c.AlunoId == req.AlunoId);

        if (confirmacao != null)
        {
            confirmacao.Vai = req.Vai;
            confirmacao.RespondidoPorUtilizadorId = utilizadorId;
            confirmacao.RespondidoEm = DateTime.UtcNow;
        }
        else
        {
            confirmacao = new ConfirmacaoPresenca
            {
                SessaoAulaId = id,
                AlunoId = req.AlunoId,
                Vai = req.Vai,
                RespondidoPorUtilizadorId = utilizadorId,
                RespondidoEm = DateTime.UtcNow
            };

            _db.ConfirmacaoPresenca.Add(confirmacao);
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    // ===============================
    // GET /api/sessoes/{id}/confirmacoes
    // ===============================
    [HttpGet("{id}/confirmacoes")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> ListarConfirmacoes(int id)
    {
        var confirmacoes = await _db.ConfirmacaoPresenca
            .Where(c => c.SessaoAulaId == id)
            .Select(c => new
            {
                c.AlunoId,
                NomeAluno = c.Aluno.Nome,
                c.Vai,
                c.RespondidoEm
            })
            .ToListAsync();

        return Ok(confirmacoes);
    }

    // ===============================
    // POST /api/sessoes/{id}/presencas
    // ===============================
    [HttpPost("{id}/presencas")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> MarcarPresencas(int id, [FromBody] List<MarcarPresenca> req)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        foreach (var item in req)
        {
            var presenca = await _db.Presencas
                .FirstOrDefaultAsync(p => p.SessaoAulaId == id && p.AlunoId == item.AlunoId);

            if (presenca != null)
            {
                presenca.Presente = item.Presente;
                presenca.MarcadoPorUtilizadorId = utilizadorId;
                presenca.MarcadoEm = DateTime.UtcNow;
            }
            else
            {
                presenca = new Presenca
                {
                    SessaoAulaId = id,
                    AlunoId = item.AlunoId,
                    Presente = item.Presente,
                    MarcadoPorUtilizadorId = utilizadorId,
                    MarcadoEm = DateTime.UtcNow
                };

                _db.Presencas.Add(presenca);
            }
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    // ===============================
    // GET /api/sessoes/{id}/presencas
    // ===============================
    [HttpGet("{id}/presencas")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> ListarPresencas(int id)
    {
        var presencas = await _db.Presencas
            .Where(p => p.SessaoAulaId == id)
            .Select(p => new PresencaResponse
            {
                AlunoId = p.AlunoId,
                NomeAluno = p.Aluno.Nome,
                Presente = p.Presente,
                MarcadoEm = p.MarcadoEm
            })
            .ToListAsync();

        return Ok(presencas);
    }
}