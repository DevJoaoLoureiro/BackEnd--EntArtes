using EscolaDanca.Data;
using EscolaDanca.DTOs;
using EscolaDanca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/pagamentos")]
public class PagamentosController : ControllerBase
{
    private readonly AppDbContext _db;

    public PagamentosController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/pagamentos
    [HttpGet]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? estado,
        [FromQuery] string? tipo,
        [FromQuery] int? alunoId)
    {
        var query =
            from p in _db.Pagamentos
            join a in _db.Alunos on p.AlunoId equals a.Id
            select new
            {
                p.Id,
                p.AlunoId,
                NomeAluno = a.Nome,
                p.Referencia,
                p.Valor,
                p.Estado,
                p.DataPagamento,
                p.CriadoEm,
                p.SessaoId,
                p.Tipo,
                p.Descricao,
                p.Mes,
                p.Ano
            };

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.Estado == estado.ToUpper());

        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(p => p.Tipo == tipo.ToUpper());

        if (alunoId.HasValue)
            query = query.Where(p => p.AlunoId == alunoId.Value);

        var pagamentos = await query
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

        return Ok(pagamentos);
    }

    // GET /api/pagamentos/meus
    [HttpGet("meus")]
    [Authorize(Roles = "ENCARREGADO,ALUNO")]
    public async Task<IActionResult> Meus()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (!int.TryParse(userIdClaim, out var utilizadorId))
            return Unauthorized();

        List<int> alunoIds = new();

        if (role == "ENCARREGADO")
        {
            var responsavel = await _db.Responsaveis
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UtilizadorId == utilizadorId);

            if (responsavel == null)
                return Ok(new List<object>());

            alunoIds = await _db.AlunoResponsaveis
                .Where(ar => ar.ResponsavelId == responsavel.Id)
                .Select(ar => ar.AlunoId)
                .ToListAsync();
        }
        else if (role == "ALUNO")
        {
            alunoIds = await _db.Alunos
                .Where(a => a.UtilizadorId == utilizadorId)
                .Select(a => a.Id)
                .ToListAsync();
        }

        var pagamentos = await (
            from p in _db.Pagamentos
            join a in _db.Alunos on p.AlunoId equals a.Id
            where alunoIds.Contains(p.AlunoId)
            orderby p.CriadoEm descending
            select new
            {
                p.Id,
                p.AlunoId,
                NomeAluno = a.Nome,
                p.Referencia,
                p.Valor,
                p.Estado,
                p.DataPagamento,
                p.CriadoEm,
                p.SessaoId,
                p.Tipo,
                p.Descricao,
                p.Mes,
                p.Ano
            }
        ).ToListAsync();

        return Ok(pagamentos);
    }

    // POST /api/pagamentos
    [HttpPost]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Create([FromBody] CriarPagamentoRequest req)
    {
        if (req.AlunoId <= 0)
            return BadRequest("Aluno obrigatório.");

        if (req.Valor <= 0)
            return BadRequest("Valor tem de ser maior que zero.");

        var alunoExiste = await _db.Alunos.AnyAsync(a => a.Id == req.AlunoId);
        if (!alunoExiste)
            return BadRequest("Aluno não encontrado.");

        var tipo = string.IsNullOrWhiteSpace(req.Tipo)
            ? "MANUAL"
            : req.Tipo.Trim().ToUpper();

        var pagamento = new Pagamento
        {
            AlunoId = req.AlunoId,
            Valor = req.Valor,
            Tipo = tipo,
            Descricao = string.IsNullOrWhiteSpace(req.Descricao) ? null : req.Descricao.Trim(),
            SessaoId = req.SessaoId,
            Mes = req.Mes,
            Ano = req.Ano,
            Referencia = $"PAG-{DateTime.UtcNow:yyyyMMddHHmmss}-A{req.AlunoId}",
            Estado = "PENDENTE",
            CriadoEm = DateTime.UtcNow
        };

        _db.Pagamentos.Add(pagamento);
        await _db.SaveChangesAsync();

        return Ok(pagamento);
    }

}