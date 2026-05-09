using EscolaDanca.Data;
using EscolaDanca.DTOs;
using EscolaDanca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


[ApiController]
[Route("api/eventos")]
public class EventosController : ControllerBase
{
    private readonly AppDbContext _db;
    public EventosController(AppDbContext db) => _db = db;




    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var eventos = await _db.Eventos
            .Include(e => e.Professores) // ← adiciona
            .OrderByDescending(e => e.DataInicio)
            .Select(e => new
            {
                e.Id,
                e.Titulo,
                e.Descricao,
                e.DataInicio,
                e.DataFim,
                e.Local,
                e.Publico,
                e.CriadoEm,
                ProfessoresIds = e.Professores.Select(p => p.ProfessorUtilizadorId).ToList() // ← adiciona
            })
            .ToListAsync();

        return Ok(eventos);
    }

    // GET /api/eventos/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var perfil = User.FindFirst("perfil")?.Value ?? "";

        var e = await _db.Eventos.FindAsync(id);
        if (e == null) return NotFound();

        if (perfil != "ADMIN" && perfil != "SUPER_ADMIN" && !e.Publico)
            return Forbid();

        return Ok(e);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN, PROFESSOR")]
    public async Task<IActionResult> Create([FromBody] EventoRequest req)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var evento = new Evento
        {
            Titulo = req.Titulo,
            Descricao = req.Descricao,
            DataInicio = req.DataInicio,
            DataFim = req.DataFim,
            Local = req.Local,
            Publico = req.Publico,
            CriadoPorUtilizadorId = utilizadorId,
            CriadoEm = DateTime.UtcNow
        };

        _db.Eventos.Add(evento);
        await _db.SaveChangesAsync();

        // ✅ Guardar professores do evento
        if (req.ProfessoresIds != null && req.ProfessoresIds.Any())
        {
            foreach (var profId in req.ProfessoresIds)
            {
                _db.EventoProfessores.Add(new EventoProfessor
                {
                    EventoId = evento.Id,
                    ProfessorUtilizadorId = profId
                });
            }
            await _db.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetById), new { id = evento.Id }, evento);
    }


    [HttpPost("{id}/inscrever-educandos")]
    [Authorize(Roles = "ENCARREGADO")]
    public async Task<IActionResult> InscreverEducandos(int id, [FromBody] InscreverEventoRequest req)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var utilizadorId))
            return Unauthorized();

        var evento = await _db.Eventos.FindAsync(id);
        if (evento == null)
            return NotFound("Evento não encontrado.");

        if (req.AlunoIds == null || !req.AlunoIds.Any())
            return BadRequest("Tem de indicar pelo menos um educando.");

        var responsavel = await _db.Responsaveis
            .FirstOrDefaultAsync(r => r.UtilizadorId == utilizadorId);

        if (responsavel == null)
            return BadRequest("Responsável não encontrado.");

        foreach (var alunoId in req.AlunoIds)
        {
          
            var ligado = await _db.AlunoResponsaveis
                .AnyAsync(ar => ar.ResponsavelId == responsavel.Id && ar.AlunoId == alunoId);

            if (!ligado)
                continue; // ignora alunos inválidos

            // evitar duplicados
            var jaInscrito = await _db.EventoInscricoes
                .AnyAsync(x => x.EventoId == id && x.AlunoId == alunoId);

            if (jaInscrito)
                continue;

            _db.EventoInscricoes.Add(new EventoInscricao
            {
                EventoId = id,
                AlunoId = alunoId,
                InscritoPorUtilizadorId = utilizadorId,
                InscritoEm = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Educandos inscritos com sucesso." });
    }



    [HttpGet("{id}/inscritos")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> GetInscritos(int id)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var evento = await _db.Eventos
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
       
        if (evento == null)
            return NotFound("Evento não encontrado.");

        // Se for professor, só pode ver inscritos de eventos em que está associado
        if (role == "PROFESSOR")
        {
            var pertenceAoEvento = await _db.EventoProfessores
                .AnyAsync(ep => ep.EventoId == id && ep.ProfessorUtilizadorId == utilizadorId);

            if (!pertenceAoEvento)
                return Forbid();
        }

        var inscritos = await _db.EventoInscricoes
            .Where(i => i.EventoId == id)
            .Select(i => new
            {
                i.Id,
                i.AlunoId,
                NomeAluno = i.Aluno.Nome,
                i.InscritoEm,
                InscritoPor = i.InscritoPorUtilizador.Nome,
                i.Avaliacao
            })
            .OrderBy(i => i.NomeAluno)
            .ToListAsync();

        return Ok(inscritos);
    }





    [HttpPost("{id}/inscrever-me")]
    [Authorize(Roles = "ALUNO")]
    public async Task<IActionResult> InscreverMe(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var utilizadorId))
            return Unauthorized();

        var evento = await _db.Eventos.FindAsync(id);
        if (evento == null)
            return NotFound("Evento não encontrado.");

        var aluno = await _db.Alunos
            .FirstOrDefaultAsync(a => a.UtilizadorId == utilizadorId);

        if (aluno == null)
            return BadRequest("Este utilizador não está associado a um aluno.");

        // 🔥 validar idade
        if (!aluno.DataNascimento.HasValue)
            return BadRequest("Aluno sem data de nascimento.");

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var nascimento = aluno.DataNascimento.Value;

        var idade = hoje.Year - nascimento.Year;
        if (nascimento > hoje.AddYears(-idade))
            idade--;

        if (idade < 18)
            return Forbid("Apenas alunos maiores de idade podem inscrever-se.");

        // evitar duplicados
        var jaInscrito = await _db.EventoInscricoes
            .AnyAsync(x => x.EventoId == id && x.AlunoId == aluno.Id);

        if (jaInscrito)
            return BadRequest("Já estás inscrito neste evento.");

        _db.EventoInscricoes.Add(new EventoInscricao
        {
            EventoId = id,
            AlunoId = aluno.Id,
            InscritoPorUtilizadorId = utilizadorId,
            InscritoEm = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return Ok(new { message = "Inscrição efetuada com sucesso." });
    }





    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN, PROFESSOR")]
    public async Task<IActionResult> Update(int id, [FromBody] EventoRequest req)
    {
        var evento = await _db.Eventos.FindAsync(id);
        if (evento == null) return NotFound();

        evento.Titulo = req.Titulo;
        evento.Descricao = req.Descricao;
        evento.DataInicio = req.DataInicio;
        evento.DataFim = req.DataFim;
        evento.Local = req.Local;
        evento.Publico = req.Publico;

        // Upsert professores:
        // - mantém os que já existem e continuam marcados (ID não muda)
        // - remove os que foram desmarcados
        // - adiciona apenas os novos
        var existentes = await _db.EventoProfessores
            .Where(ep => ep.EventoId == id)
            .ToListAsync();

        var idsExistentes = existentes.Select(ep => ep.ProfessorUtilizadorId).ToList();
        var idsNovos = req.ProfessoresIds ?? new List<int>();

        var paraRemover = existentes
            .Where(ep => !idsNovos.Contains(ep.ProfessorUtilizadorId))
            .ToList();
        _db.EventoProfessores.RemoveRange(paraRemover);

        var paraAdicionar = idsNovos
            .Where(profId => !idsExistentes.Contains(profId))
            .ToList();
        foreach (var profId in paraAdicionar)
        {
            _db.EventoProfessores.Add(new EventoProfessor
            {
                EventoId = id,
                ProfessorUtilizadorId = profId
            });
        }

        await _db.SaveChangesAsync();
        return Ok(evento);
    }


    //verificar se a data de avaliacao for maior ou menor que a data(feito)


    [HttpPatch("inscricoes/{id}/avaliar")]
    [Authorize(Roles = "PROFESSOR,ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Avaliar(int id, [FromBody] AvaliarEventoRequest req)
    {
        try
        {
            if (req == null)
                return BadRequest("Dados inválidos.");

            if (req.Avaliacao < 0 || req.Avaliacao > 5)
                return BadRequest("A avaliação tem de estar entre 0 e 5.");

            var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
                return Unauthorized();

            var inscricao = await _db.EventoInscricoes
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inscricao == null)
                return NotFound("Inscrição não encontrada.");

            var evento = await _db.Eventos
                .FirstOrDefaultAsync(e => e.Id == inscricao.EventoId);

            if (evento == null)
                return NotFound("Evento não encontrado.");

            // Só permite avaliar depois da data/hora de fim do evento
            if (!evento.DataFim.HasValue)
                return BadRequest("O evento não tem data de fim definida.");

            if (DateTime.UtcNow < evento.DataFim.Value.ToUniversalTime())
                return BadRequest("Só é possível avaliar depois do evento terminar.");

            if (role == "PROFESSOR")
            {
                var pertence = await _db.EventoProfessores
                    .AnyAsync(ep => ep.EventoId == inscricao.EventoId && ep.ProfessorUtilizadorId == utilizadorId);

                if (!pertence)
                    return Forbid();
            }

            inscricao.Avaliacao = req.Avaliacao;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Avaliação guardada com sucesso." });
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


    // DELETE /api/eventos/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN, PROFESSOR")]
    public async Task<IActionResult> Delete(int id)
    {
        var evento = await _db.Eventos.FindAsync(id);
        if (evento == null) return NotFound();

        _db.Eventos.Remove(evento);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}