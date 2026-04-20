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

        // sessão normal precisa de turma
        if (!req.InscricaoAberta && !req.TurmaId.HasValue)
            return BadRequest("Sessão normal precisa de turma.");

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
            TurmaId = req.InscricaoAberta ? null : req.TurmaId,
            InscricaoAberta = req.InscricaoAberta
        };

        _db.SessoesAula.Add(sessao);
        await _db.SaveChangesAsync();

        // só copia alunos automaticamente se NÃO for coaching
        if (!req.InscricaoAberta && req.TurmaId.HasValue)
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
            sessao.TurmaId,
            sessao.InscricaoAberta
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
    [HttpPost("{id}/confirmar")]
    [Authorize]
    public async Task<IActionResult> ConfirmarPresenca(int id, [FromBody] ConfirmarSessaoDto req)
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
                RespondidoEm = DateTime.UtcNow,
                CriadoEm = DateTime.UtcNow,
               
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


    [HttpGet("abertas")]
    [Authorize]
    public async Task<IActionResult> ListarSessoesAbertas()
    {
        var sessoes = await _db.SessoesAula
            .AsNoTracking()
            .Where(s => s.InscricaoAberta && !s.FoiDada)
            .OrderBy(s => s.Inicio)
            .Select(s => new
            {
                s.Id,
                s.Inicio,
                s.Fim,
                s.Estado,
                s.Sumario,
                s.MaxAlunos,
                s.InscricaoAberta
            })
            .ToListAsync();

        return Ok(sessoes);
    }



    [HttpGet("pendentes-confirmacao")]
    [Authorize(Roles = "ENCARREGADO,ALUNO")]
    public async Task<IActionResult> PendentesConfirmacao()
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var alunoIds = new List<int>();

        if (role == "ALUNO")
        {
            alunoIds = await _db.Alunos
                .Where(a => a.UtilizadorId == utilizadorId)
                .Select(a => a.Id)
                .ToListAsync();
        }
        else if (role == "ENCARREGADO")
        {
            var responsavel = await _db.Responsaveis
                .FirstOrDefaultAsync(r => r.UtilizadorId == utilizadorId);

            if (responsavel == null)
                return Ok(new List<object>());

            alunoIds = await _db.AlunoResponsaveis
                .Where(ar => ar.ResponsavelId == responsavel.Id)
                .Select(ar => ar.AlunoId)
                .ToListAsync();
        }

        if (!alunoIds.Any())
            return Ok(new List<object>());

        var pendentes = await _db.SessaoAlunos
            .AsNoTracking()
            .Where(sa => alunoIds.Contains(sa.AlunoId))
            .Where(sa => !_db.ConfirmacaoPresenca
                .Any(c => c.SessaoAulaId == sa.SessaoAulaId && c.AlunoId == sa.AlunoId))
            .Select(sa => new
            {
                sessaoId = sa.SessaoAulaId,
                alunoId = sa.AlunoId,
                alunoNome = sa.Aluno != null ? sa.Aluno.Nome : "",
                dataInicio = sa.Sessao != null ? sa.Sessao.Inicio : DateTime.MinValue,
                dataFim = sa.Sessao != null ? sa.Sessao.Fim : DateTime.MinValue,
                turmaNome = sa.Sessao != null && sa.Sessao.Turma != null
                    ? sa.Sessao.Turma.Nome
                    : "",
                professorNome = sa.Sessao != null && sa.Sessao.Professor != null
                    ? sa.Sessao.Professor.Nome
                    : ""
            })
            .OrderBy(x => x.dataInicio)
            .ToListAsync();

        return Ok(pendentes);
    }



    [HttpPost("{id}/inscrever-me")]
    [Authorize(Roles = "ALUNO")]
    public async Task<IActionResult> InscreverMe(int id)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var sessao = await _db.SessoesAula
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sessao == null)
            return NotFound("Sessão não encontrada.");

        if (!sessao.InscricaoAberta)
            return BadRequest("Esta sessão não aceita inscrições livres.");

        if (sessao.FoiDada)
            return BadRequest("A sessão já foi dada.");

        if (sessao.Estado != null && sessao.Estado.ToUpper() != "AGENDADA")
            return BadRequest("A sessão não está disponível para inscrição.");

        var aluno = await _db.Alunos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UtilizadorId == utilizadorId && a.Ativo);

        if (aluno == null)
            return BadRequest("Este utilizador não está associado a um aluno ativo.");

        var jaInscrito = await _db.SessaoAlunos
            .AnyAsync(sa => sa.SessaoAulaId == id && sa.AlunoId == aluno.Id);

        if (jaInscrito)
            return BadRequest("Já estás inscrito nesta sessão.");

        if (sessao.MaxAlunos.HasValue)
        {
            var totalInscritos = await _db.SessaoAlunos
                .CountAsync(sa => sa.SessaoAulaId == id);

            if (totalInscritos >= sessao.MaxAlunos.Value)
                return BadRequest("A sessão já atingiu o número máximo de alunos.");
        }

        var inscricao = new SessaoAluno
        {
            SessaoAulaId = id,
            AlunoId = aluno.Id,
            AdicionadoPorUtilizadorId = utilizadorId,
            AdicionadoEm = DateTime.UtcNow
        };

        _db.SessaoAlunos.Add(inscricao);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest("Não foi possível concluir a inscrição. Podes já estar inscrito ou a sessão ter ficado sem vagas.");
        }

        return Ok(new
        {
            message = "Inscrição efetuada com sucesso."
        });
    }
    [HttpPost("{id}/inscrever-educandos")]
    [Authorize(Roles = "ENCARREGADO")]
    public async Task<IActionResult> InscreverEducandos(int id, [FromBody] InscreverAlunoRequest req)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var sessao = await _db.SessoesAula
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sessao == null)
            return NotFound("Sessão não encontrada.");

        if (!sessao.InscricaoAberta)
            return BadRequest("Esta sessão não aceita inscrições livres.");

        if (sessao.FoiDada)
            return BadRequest("A sessão já foi dada.");

        if (sessao.Estado != null && sessao.Estado.ToUpper() != "AGENDADA")
            return BadRequest("A sessão não está disponível para inscrição.");

        if (req.AlunoIds == null || !req.AlunoIds.Any())
            return BadRequest("Tem de selecionar pelo menos um educando.");

        var responsavel = await _db.Responsaveis
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UtilizadorId == utilizadorId);

        if (responsavel == null)
            return BadRequest("Responsável não encontrado.");

        var alunoIdsPermitidos = await _db.AlunoResponsaveis
            .Where(ar => ar.ResponsavelId == responsavel.Id)
            .Select(ar => ar.AlunoId)
            .ToListAsync();

        var idsValidos = req.AlunoIds
            .Where(idAluno => alunoIdsPermitidos.Contains(idAluno))
            .Distinct()
            .ToList();

        if (!idsValidos.Any())
            return Forbid();

        var jaInscritos = await _db.SessaoAlunos
            .Where(sa => sa.SessaoAulaId == id && idsValidos.Contains(sa.AlunoId))
            .Select(sa => sa.AlunoId)
            .ToListAsync();

        var idsParaInserir = idsValidos
            .Where(x => !jaInscritos.Contains(x))
            .ToList();

        if (!idsParaInserir.Any())
        {
            return BadRequest(new
            {
                message = "Todos os educandos selecionados já estão inscritos nesta sessão.",
                inscritos = 0,
                jaInscritos = jaInscritos.Count
            });
        }

        if (sessao.MaxAlunos.HasValue)
        {
            var totalInscritosAtual = await _db.SessaoAlunos
                .CountAsync(sa => sa.SessaoAulaId == id);

            var vagasDisponiveis = sessao.MaxAlunos.Value - totalInscritosAtual;

            if (vagasDisponiveis <= 0)
            {
                return BadRequest(new
                {
                    message = "A sessão já atingiu o número máximo de alunos."
                });
            }

            if (idsParaInserir.Count > vagasDisponiveis)
            {
                idsParaInserir = idsParaInserir.Take(vagasDisponiveis).ToList();
            }
        }

        foreach (var alunoId in idsParaInserir)
        {
            _db.SessaoAlunos.Add(new SessaoAluno
            {
                SessaoAulaId = id,
                AlunoId = alunoId,
                AdicionadoPorUtilizadorId = utilizadorId,
                AdicionadoEm = DateTime.UtcNow
            });
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new
            {
                message = "Não foi possível concluir a inscrição. Alguns educandos podem já estar inscritos ou a sessão pode ter ficado sem vagas."
            });
        }

        return Ok(new
        {
            message = "Educandos inscritos com sucesso.",
            inscritos = idsParaInserir.Count,
            jaInscritos = jaInscritos.Count,
            totalPedidos = req.AlunoIds.Count
        });
    }


}