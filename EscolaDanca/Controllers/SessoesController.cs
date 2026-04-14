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

    public SessoesController(AppDbContext db)
    {
        _db = db;
    }

    // =========================================
    // GET /api/sessoes
    // =========================================
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var query = _db.SessoesAula.AsNoTracking().AsQueryable();

        // ADMIN e SUPER_ADMIN veem tudo
        if (role != "ADMIN" && role != "SUPER_ADMIN")
        {
            query = query.Where(s => s.CriadoPorUtilizadorId == utilizadorId);
        }

        var sessoes = await query
            .OrderByDescending(s => s.Inicio)
            .Select(s => new
            {
                s.Id,
                s.ProfessorUtilizadorId,
                ProfessorNome = s.Professor != null ? s.Professor.Nome : null,
                s.Inicio,
                s.Fim,
                s.Estado,
                s.CriadoPorUtilizadorId,
                s.CriadoEm,
                s.TipoAulaId,
                s.EstudioId,
                s.MaxAlunos,
                s.Sumario,
                s.FoiDada,
                s.MotivoFaltaProfessor
            })
            .ToListAsync();

        return Ok(sessoes);
    }



    [HttpDelete("{id}/alunos/{alunoId}")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> RemoverAluno(int id, int alunoId)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var sessao = await _db.SessoesAula.FindAsync(id);
        if (sessao == null)
            return NotFound("Sessão não encontrada.");

        if (role != "ADMIN" && role != "SUPER_ADMIN" && sessao.CriadoPorUtilizadorId != utilizadorId)
            return Forbid();

        var item = await _db.SessaoAlunos
            .FirstOrDefaultAsync(x => x.SessaoAulaId == id && x.AlunoId == alunoId);

        if (item == null)
            return NotFound("Aluno não está nessa sessão.");

        _db.SessaoAlunos.Remove(item);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Aluno removido da sessão." });
    }



    [HttpGet("{id}/alunos")]
    [Authorize]
    public async Task<IActionResult> ListarAlunosSessao(int id)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var sessao = await _db.SessoesAula.FindAsync(id);
        if (sessao == null)
            return NotFound("Sessão não encontrada.");

        if (role != "ADMIN" && role != "SUPER_ADMIN" && sessao.CriadoPorUtilizadorId != utilizadorId)
            return Forbid();

        var alunos = await _db.SessaoAlunos
            .Where(x => x.SessaoAulaId == id)
            .Select(x => new
            {
                x.AlunoId,
                NomeAluno = x.Aluno!.Nome
            })
            .ToListAsync();

        return Ok(alunos);
    }






    [HttpPost("{id}/alunos")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> AdicionarAluno(int id, [FromBody] AddAlunoSessaoDto req)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var sessao = await _db.SessoesAula.FindAsync(id);
        if (sessao == null)
            return NotFound("Sessão não encontrada.");

        if (role != "ADMIN" && role != "SUPER_ADMIN" && sessao.CriadoPorUtilizadorId != utilizadorId)
            return Forbid();

        var alunoExiste = await _db.Alunos.AnyAsync(a => a.Id == req.AlunoId);
        if (!alunoExiste)
            return BadRequest("Aluno inválido.");

        var jaExiste = await _db.SessaoAlunos
            .AnyAsync(x => x.SessaoAulaId == id && x.AlunoId == req.AlunoId);

        if (jaExiste)
            return BadRequest("Esse aluno já está na sessão.");

        var item = new SessaoAluno
        {
            SessaoAulaId = id,
            AlunoId = req.AlunoId,
            AdicionadoPorUtilizadorId = utilizadorId,
            AdicionadoEm = DateTime.UtcNow
        };

        _db.SessaoAlunos.Add(item);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Aluno adicionado à sessão com sucesso." });
    }




    // =========================================
    // GET /api/sessoes/{id}
    // =========================================
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var sessao = await _db.SessoesAula
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.ProfessorUtilizadorId,
                ProfessorNome = s.Professor != null ? s.Professor.Nome : null,
                s.Inicio,
                s.Fim,
                s.Estado,
                s.CriadoPorUtilizadorId,
                s.CriadoEm,
                s.TipoAulaId,
                s.EstudioId,
                s.MaxAlunos,
                s.Sumario,
                s.FoiDada,
                s.MotivoFaltaProfessor
            })
            .FirstOrDefaultAsync();

        if (sessao == null)
            return NotFound("Sessão não encontrada.");

        return Ok(sessao);
    }

    // =========================================
    // POST /api/sessoes
    // =========================================




    [HttpPost]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> Create([FromBody] CreateSessao req)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        if (req.DataFim <= req.DataInicio)
            return BadRequest("A data/hora de fim tem de ser maior que a de início.");

        var professorId = req.ProfessorUtilizadorId ?? utilizadorId;

        var sessao = new SessaoAula
        {
            ProfessorUtilizadorId = professorId,
            Inicio = req.DataInicio,
            Fim = req.DataFim,
            Estado = "AGENDADA",
            CriadoPorUtilizadorId = utilizadorId,
            CriadoEm = DateTime.UtcNow,
            TipoAulaId = req.TipoAulaId,
            EstudioId = req.EstudioId,
            MaxAlunos = req.MaxAlunos,
            Sumario = req.Sumario,
            FoiDada = false,
            MotivoFaltaProfessor = null,
            TurmaId = req.TurmaId
        };

        _db.SessoesAula.Add(sessao);
        await _db.SaveChangesAsync();

        // AQUI
        if (req.TurmaId.HasValue)
        {
            var alunosDaTurma = await _db.TurmaAlunos
                .Where(x => x.TurmaId == req.TurmaId.Value)
                .Select(x => x.AlunoId)
                .ToListAsync();

            foreach (var alunoId in alunosDaTurma)
            {
                _db.SessaoAlunos.Add(new SessaoAluno
                {
                    SessaoAulaId = sessao.Id,
                    AlunoId = alunoId,
                    AdicionadoPorUtilizadorId = utilizadorId,
                    AdicionadoEm = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetById), new { id = sessao.Id }, new
        {
            sessao.Id,
            sessao.ProfessorUtilizadorId,
            sessao.Inicio,
            sessao.Fim,
            sessao.Estado,
            sessao.CriadoPorUtilizadorId,
            sessao.CriadoEm,
            sessao.TipoAulaId,
            sessao.EstudioId,
            sessao.MaxAlunos,
            sessao.Sumario,
            sessao.FoiDada,
            sessao.MotivoFaltaProfessor,
            sessao.TurmaId
        });
    }


    [HttpPatch("{id}/terminar")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> TerminarAula(int id)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var sessao = await _db.SessoesAula.FindAsync(id);
        if (sessao == null)
            return NotFound("Sessão não encontrada.");

        // Só ADMIN / SUPER_ADMIN ou criador da aula
        if (role != "ADMIN" && role != "SUPER_ADMIN" && sessao.CriadoPorUtilizadorId != utilizadorId)
            return Forbid();

        sessao.FoiDada = true;
        sessao.Estado = "TERMINADA";

        await _db.SaveChangesAsync();

        return Ok(new { message = "Aula terminada com sucesso." });
    }
    // =========================================
    // POST /api/sessoes/{id}/confirmar
    // EE confirma ida à aula
    // =========================================
    [HttpPost("{id}/confirmar")]
    [Authorize]
    public async Task<IActionResult> ConfirmarPresenca(int id, [FromBody] ConfirmacaoPresenca req)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var sessaoExiste = await _db.SessoesAula.AnyAsync(s => s.Id == id);
        if (!sessaoExiste)
            return NotFound("Sessão não encontrada.");

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
        return Ok(new { message = "Confirmação registada com sucesso." });
    }

    // =========================================
    // GET /api/sessoes/{id}/confirmacoes
    // =========================================
    [HttpGet("{id}/confirmacoes")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> ListarConfirmacoes(int id)
    {
        var sessaoExiste = await _db.SessoesAula.AnyAsync(s => s.Id == id);
        if (!sessaoExiste)
            return NotFound("Sessão não encontrada.");

        var confirmacoes = await _db.ConfirmacaoPresenca
            .AsNoTracking()
            .Where(c => c.SessaoAulaId == id)
            .Select(c => new
            {
                c.Id,
                c.AlunoId,
                NomeAluno = c.Aluno.Nome,
                c.Vai,
                c.RespondidoPorUtilizadorId,
                c.RespondidoEm
            })
            .ToListAsync();

        return Ok(confirmacoes);
    }

    // =========================================
    // POST /api/sessoes/{id}/presencas
    // Professor marca presenças
    // =========================================
    [HttpPost("{id}/presencas")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> MarcarPresencas(int id, [FromBody] List<MarcarPresenca> req)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var sessaoExiste = await _db.SessoesAula.AnyAsync(s => s.Id == id);
        if (!sessaoExiste)
            return NotFound("Sessão não encontrada.");

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
        return Ok(new { message = "Presenças registadas com sucesso." });
    }

    // =========================================
    // GET /api/sessoes/{id}/presencas
    // =========================================
    [HttpGet("{id}/presencas")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> ListarPresencas(int id)
    {
        var sessaoExiste = await _db.SessoesAula.AnyAsync(s => s.Id == id);
        if (!sessaoExiste)
            return NotFound("Sessão não encontrada.");

        var presencas = await _db.Presencas
            .AsNoTracking()
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