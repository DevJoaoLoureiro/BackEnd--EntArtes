using EscolaDanca.Data;
using EscolaDanca.DTOs;
using EscolaDanca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/turmas")]
public class TurmasController : ControllerBase
{
    private readonly AppDbContext _db;
    public TurmasController(AppDbContext db) => _db = db;




    // POST /api/turmas
    [HttpPost ]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN, PROFESSOR")]
    public async Task<IActionResult> Criar([FromBody] CriarTurmaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            return BadRequest("Nome é obrigatório.");



        if (dto.ProfessorId.HasValue)
        {
            var professorValido = await _db.Utilizadores.AnyAsync(u =>
                u.Id == dto.ProfessorId.Value &&
                u.Ativo &&
                u.Perfil == "PROFESSOR"
            );

            if (!professorValido)
                return BadRequest("Professor inválido.");
        }
        // cria turma
        var turma = new Turma
        {
            Nome = dto.Nome.Trim(),
            Capacidade = dto.Capacidade ?? 0,
            Ativa = true,
            CriadoEm = DateTime.UtcNow
        };

        _db.Turmas.Add(turma);
        await _db.SaveChangesAsync();

        // se veio professorId, cria ligação na TurmaProfessor
        if (dto.ProfessorId.HasValue)
        {
            var professorValido = await _db.Utilizadores.AnyAsync(u =>
                u.Id == dto.ProfessorId.Value &&
                u.Ativo &&
                u.Perfil == "PROFESSOR"
            );

            if (!professorValido)
                return BadRequest("Professor inválido.");

            // se a tua TurmaProfessor permitir apenas 1 por turma, remove existentes (segurança)
            var existentes = await _db.TurmaProfessores
                .Where(tp => tp.TurmaId == turma.Id)
                .ToListAsync();

            if (existentes.Any())
                _db.TurmaProfessores.RemoveRange(existentes);

            _db.TurmaProfessores.Add(new TurmaProfessor
            {
                TurmaId = turma.Id,
                ProfessorUtilizadorId = dto.ProfessorId.Value
            });

            await _db.SaveChangesAsync();
        }

        return Ok(new { turma.Id });
    }






    [HttpGet]

    public async Task<IActionResult> Listar()
    {
        var turmas = await _db.Turmas
            .AsNoTracking()
            .OrderBy(t => t.Nome)
            .Select(t => new
            {
                t.Id,
                t.Nome,
                t.Nivel,
                t.Capacidade,
                t.Ativa,
                t.CriadoEm,

                ProfessorId = _db.TurmaProfessores
                    .Where(tp => tp.TurmaId == t.Id)
                    .Select(tp => (int?)tp.ProfessorUtilizadorId)
                    .FirstOrDefault(),

                ProfessorNome = _db.TurmaProfessores
                    .Where(tp => tp.TurmaId == t.Id)
                    .Join(_db.Utilizadores,
                        tp => tp.ProfessorUtilizadorId,
                        u => u.Id,
                        (tp, u) => u.Nome)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(turmas);
    }
}
