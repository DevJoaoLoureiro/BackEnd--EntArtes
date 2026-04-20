using EscolaDanca.Data;
using EscolaDanca.DTOs;
using EscolaDanca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

[ApiController]
[Route("api/turmas")]
public class TurmasController : ControllerBase
{
    private readonly AppDbContext _db;

    public TurmasController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var query = _db.Turmas.AsNoTracking().AsQueryable();

        if (role != "ADMIN" && role != "SUPER_ADMIN")
        {
            query = query.Where(t => t.ProfessorUtilizadorId == userId);
        }

        var turmas = await query
        .OrderBy(t => t.Nome)
        .Select(t => new TurmaResponseDto
        {
            Id = t.Id,
            Nome = t.Nome,
    

            ProfessorUtilizadorId = t.ProfessorUtilizadorId,
            ProfessorNome = _db.Utilizadores
        .Where(u => u.Id == t.ProfessorUtilizadorId)
        .Select(u => u.Nome)
        .FirstOrDefault(),
            Ativa = t.Ativa
        })
        .ToListAsync();

        return Ok(turmas);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> Create([FromBody] CreateTurmaDto req)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(req.Nome))
            return BadRequest("Nome obrigatório.");

        var turma = new Turma
        {
            Nome = req.Nome.Trim(),
            ProfessorUtilizadorId = req.ProfessorUtilizadorId ?? userId,
            TipoAulaId = req.TipoAulaId,
            Ativa = true,
            CriadaEm = DateTime.UtcNow
        };

        _db.Turmas.Add(turma);
        await _db.SaveChangesAsync();

        return Ok(turma);
    }

    [HttpGet("{id}/alunos")]
    [Authorize]
    public async Task<IActionResult> GetAlunos(int id)
    {
        var alunos = await _db.TurmaAlunos
            .Where(x => x.TurmaId == id)
            .Select(x => new
            {
                x.AlunoId,
                NomeAluno = x.Aluno != null ? x.Aluno.Nome : null
            })
            .ToListAsync();

        return Ok(alunos);
    }

    [HttpPost("{id}/alunos")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> AddAluno(int id, [FromBody] AddTurmaAlunoDto req)
    {
        var turma = await _db.Turmas.FindAsync(id);
        if (turma == null) return NotFound("Turma não encontrada.");

        var alunoExiste = await _db.Alunos.AnyAsync(a => a.Id == req.AlunoId);
        if (!alunoExiste) return BadRequest("Aluno inválido.");

        var jaExiste = await _db.TurmaAlunos.AnyAsync(x => x.TurmaId == id && x.AlunoId == req.AlunoId);
        if (jaExiste) return BadRequest("Aluno já está na turma.");

        _db.TurmaAlunos.Add(new TurmaAluno
        {
            TurmaId = id,
            AlunoId = req.AlunoId,
            AdicionadoEm = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Aluno adicionado à turma." });
    }

    [HttpDelete("{id}/alunos/{alunoId}")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> RemoveAluno(int id, int alunoId)
    {
        var item = await _db.TurmaAlunos.FirstOrDefaultAsync(x => x.TurmaId == id && x.AlunoId == alunoId);
        if (item == null) return NotFound("Aluno não está na turma.");

        _db.TurmaAlunos.Remove(item);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Aluno removido da turma." });
    }
}