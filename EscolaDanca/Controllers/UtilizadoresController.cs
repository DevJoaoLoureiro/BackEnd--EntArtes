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
        var perfilOk = new[] { "ADMIN", "SUPER_ADMIN", "PROFESSOR", "ENCARREGADO", "ALUNO" }.Contains(req.Perfil);
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
            criado_em = DateTime.UtcNow,
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
        try
        {
            var user = await _db.Utilizadores.FindAsync(id);

            if (user == null)
                return NotFound("Utilizador não encontrado.");

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && int.Parse(userIdClaim) == id)
                return BadRequest("Não pode eliminar o próprio utilizador.");

            var aluno = await _db.Alunos.FirstOrDefaultAsync(a => a.UtilizadorId == id);
            if (aluno != null)
            {
                var ligacoesAluno = await _db.AlunoResponsaveis
                    .Where(x => x.AlunoId == aluno.Id)
                    .ToListAsync();
                _db.AlunoResponsaveis.RemoveRange(ligacoesAluno);

                var presencasAluno = await _db.Presencas
                    .Where(p => p.AlunoId == aluno.Id)
                    .ToListAsync();
                _db.Presencas.RemoveRange(presencasAluno);

                var confirmacoesAluno = await _db.ConfirmacaoPresenca
                    .Where(c => c.AlunoId == aluno.Id)
                    .ToListAsync();
                _db.ConfirmacaoPresenca.RemoveRange(confirmacoesAluno);

                var sessoesAluno = await _db.SessaoAlunos
                    .Where(sa => sa.AlunoId == aluno.Id)
                    .ToListAsync();
                _db.SessaoAlunos.RemoveRange(sessoesAluno);

                _db.Alunos.Remove(aluno);
            }

            var responsavel = await _db.Responsaveis.FirstOrDefaultAsync(r => r.UtilizadorId == id);
            if (responsavel != null)
            {
                var ligacoesResp = await _db.AlunoResponsaveis
                    .Where(x => x.ResponsavelId == responsavel.Id)
                    .ToListAsync();
                _db.AlunoResponsaveis.RemoveRange(ligacoesResp);

                _db.Responsaveis.Remove(responsavel);
            }

            var sessoesProfessor = await _db.SessoesAula
                .Where(s => s.ProfessorUtilizadorId == id || s.CriadoPorUtilizadorId == id)
                .ToListAsync();

            foreach (var s in sessoesProfessor)
            {
                var presencas = await _db.Presencas
                    .Where(p => p.SessaoAulaId == s.Id)
                    .ToListAsync();
                _db.Presencas.RemoveRange(presencas);

                var confirmacoes = await _db.ConfirmacaoPresenca
                    .Where(c => c.SessaoAulaId == s.Id)
                    .ToListAsync();
                _db.ConfirmacaoPresenca.RemoveRange(confirmacoes);

                var sessaoAlunos = await _db.SessaoAlunos
                    .Where(sa => sa.SessaoAulaId == s.Id)
                    .ToListAsync();
                _db.SessaoAlunos.RemoveRange(sessaoAlunos);
            }

            _db.SessoesAula.RemoveRange(sessoesProfessor);

            var convites = await _db.ConvitesUtilizador
                .Where(c => c.CriadoPorUtilizadorId == id)
                .ToListAsync();
            _db.ConvitesUtilizador.RemoveRange(convites);

            _db.Utilizadores.Remove(user);

            await _db.SaveChangesAsync();

            return Ok(new { message = "Utilizador removido completamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
        }
    }


    [HttpPost("registo-por-convite")]
    [AllowAnonymous]
    public async Task<IActionResult> RegistoPorConvite([FromBody] RegistoPorConviteRequest req)
    {
        // 1. Validação de dados obrigatórios
        if (string.IsNullOrWhiteSpace(req.Token) ||
            string.IsNullOrWhiteSpace(req.Nome) ||
            string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest("Dados obrigatórios em falta.");
        }

        // 2. Procurar e validar o convite
        var convite = await _db.ConvitesUtilizador

            .FirstOrDefaultAsync(c => c.Token == req.Token);

        if (convite == null) return BadRequest("Convite inválido.");
        if (convite.Usado) return BadRequest("Convite já utilizado.");
        if (convite.ExpiraEm < DateTime.UtcNow) return BadRequest("Convite expirado.");

        // 3. Validações de existência (Username e Email)
        var username = req.Username.Trim();
        if (await _db.Utilizadores.AnyAsync(u => u.Username == username))
            return BadRequest("Username já existe.");

        if (await _db.Utilizadores.AnyAsync(u => u.Email != null && u.Email.ToLower() == convite.Email.ToLower()))
            return BadRequest("Já existe uma conta com este email.");

        // 4. Validação específica para ENCARREGADO
        if (convite.Perfil == "ENCARREGADO")
        {
            if (req.Educandos == null || !req.Educandos.Any(e => !string.IsNullOrWhiteSpace(e.Nome)))
                return BadRequest("Sendo encarregado, tem de indicar o nome de pelo menos um educando.");
        }

        // 5. Instanciar o Utilizador (Sem salvar ainda!)
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

        // 6. Lógica por Perfil (ALUNO ou ENCARREGADO)
        if (user.Perfil == "ALUNO")
        {
            var aluno = new Aluno
            {
                Nome = user.Nome,
                Utilizador = user // Ligação por objeto
            };
            _db.Alunos.Add(aluno);
        }
        else if (user.Perfil == "ENCARREGADO")
        {
            var responsavel = new Responsavel
            {
                Nome = user.Nome,
                Email = user.Email,
                Utilizador = user, // Ligação por objeto
                CriadoEm = DateTime.Now
            };
            _db.Responsaveis.Add(responsavel);

            var educandosValidos = req.Educandos!
                .Where(e => !string.IsNullOrWhiteSpace(e.Nome))
                .ToList();

            foreach (var edu in educandosValidos)
            {
                var alunoEdu = new Aluno
                {
                    Nome = edu.Nome.Trim(),
                    DataNascimento = edu.DataNascimento.HasValue
                        ? DateOnly.FromDateTime(edu.DataNascimento.Value)
                        : null
                };
                _db.Alunos.Add(alunoEdu);

                // Tabela de ligação
                _db.AlunoResponsaveis.Add(new AlunoResponsavel
                {
                    Aluno = alunoEdu,
                    Responsavel = responsavel
                });
            }
        }

        // 7. Marcar convite como usado
        convite.Usado = true;

        // 8. Único Ponto de Gravação
        // Se o erro "Invalid column name" persistir em qualquer tabela, nada será gravado aqui.
        try
        {
            await _db.SaveChangesAsync();
            return Ok(new { message = "Conta criada com sucesso." });
        }
        catch (Exception ex)
        {
            // Se der erro, o InnerException dirá qual a tabela/coluna exata
            return StatusCode(500, $"Erro ao gravar na Base de Dados: {ex.InnerException?.Message ?? ex.Message}");
        }
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