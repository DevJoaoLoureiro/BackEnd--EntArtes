using EscolaDanca.Controllers;
using EscolaDanca.Models;
using EscolaDanca.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;

namespace EscolaDanca.Tests.Controllers;

public class SessoesControllerTests
{
    private static ClaimsPrincipal CriarUser(int userId, string role)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        }, "TestAuth"));
    }

    [Fact]
    public async Task InscreverMe_DeveFalhar_SeSessaoNaoExistir()
    {
        using var db = DbContextFactory.Create();
        var controller = new SessoesController(db);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CriarUser(1, "ALUNO")
            }
        };

        var result = await controller.InscreverMe(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Sessão não encontrada.", notFound.Value);
    }

    [Fact]
    public async Task InscreverMe_DeveFalhar_SeSessaoNaoAceitaInscricoes()
    {
        using var db = DbContextFactory.Create();

        db.SessoesAula.Add(new SessaoAula
        {
            Id = 1,
            InscricaoAberta = false,
            FoiDada = false,
            Estado = "AGENDADA",
            Inicio = DateTime.UtcNow.AddDays(1),
            Fim = DateTime.UtcNow.AddDays(1).AddHours(1),
            CriadoPorUtilizadorId = 10
        });

        db.Alunos.Add(new Aluno
        {
            Id = 1,
            Nome = "Aluno Teste",
            DataNascimento = new DateOnly(2010, 1, 1), 
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            UtilizadorId = 1
        });

        await db.SaveChangesAsync();

        var controller = new SessoesController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CriarUser(1, "ALUNO")
            }
        };

        var result = await controller.InscreverMe(1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Esta sessão não aceita inscrições livres.", badRequest.Value);
    }

    [Fact]
    public async Task InscreverMe_DeveFalhar_SeSessaoLotada()
    {
        using var db = DbContextFactory.Create();

        db.SessoesAula.Add(new SessaoAula
        {
            Id = 1,
            InscricaoAberta = true,
            FoiDada = false,
            Estado = "AGENDADA",
            Inicio = DateTime.UtcNow.AddDays(1),
            Fim = DateTime.UtcNow.AddDays(1).AddHours(1),
            CriadoPorUtilizadorId = 10,
            MaxAlunos = 1
        });

        db.Alunos.AddRange(
            new Aluno
            {
                Id = 1,
                Nome = "Aluno 1",
                DataNascimento = new DateOnly(2010, 1, 1), // ✅
                Ativo = true,
                CriadoEm = DateTime.UtcNow,
                UtilizadorId = 1
            },
            new Aluno
            {
                Id = 2,
                Nome = "Aluno 2",
                DataNascimento = new DateOnly(2010, 1, 1), 
                Ativo = true,
                CriadoEm = DateTime.UtcNow,
                UtilizadorId = 2
            }
        );

        db.SessaoAlunos.Add(new SessaoAluno
        {
            SessaoAulaId = 1,
            AlunoId = 2,
            AdicionadoPorUtilizadorId = 10,
            AdicionadoEm = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var controller = new SessoesController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CriarUser(1, "ALUNO")
            }
        };

        var result = await controller.InscreverMe(1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("A sessão já atingiu o número máximo de alunos.", badRequest.Value);
    }

    [Fact]
    public async Task InscreverMe_DeveInscrever_QuandoTudoEstaCorreto()
    {
        using var db = DbContextFactory.Create();

        db.SessoesAula.Add(new SessaoAula
        {
            Id = 1,
            InscricaoAberta = true,
            FoiDada = false,
            Estado = "AGENDADA",
            Inicio = DateTime.UtcNow.AddDays(1),
            Fim = DateTime.UtcNow.AddDays(1).AddHours(1),
            CriadoPorUtilizadorId = 10,
            MaxAlunos = 10
        });

        db.Alunos.Add(new Aluno
        {
            Id = 1,
            Nome = "Aluno Teste",
            DataNascimento = new DateOnly(2010, 1, 1), 
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            UtilizadorId = 1
        });

        await db.SaveChangesAsync();

        var controller = new SessoesController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CriarUser(1, "ALUNO")
            }
        };

        var result = await controller.InscreverMe(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);

        // garante que foi mesmo inserido
        Assert.Single(db.SessaoAlunos);
    }
}