using EscolaDanca.Controllers;
using EscolaDanca.DTOs;
using EscolaDanca.Models;
using EscolaDanca.Services;
using EscolaDanca.Tests.Helpers;
using FluentAssertions;
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
    public async Task CriarSessao_ComDatasInvalidas_DeveDarBadRequest()
    {
        var db = DbContextFactory.Create();

        var pagamentos = new PagamentoService(db);

        var controller = new SessoesController(db, pagamentos);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CriarUser(1, "ADMIN")
            }
        };

        var dto = new CreateSessao
        {
            DataInicio = DateTime.UtcNow,
            DataFim = DateTime.UtcNow.AddHours(-1)
        };

        var result = await controller.Create(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CriarCoaching_DeveCriarSessao()
    {
        var db = DbContextFactory.Create();

        var pagamentos = new PagamentoService(db);

        var controller = new SessoesController(db, pagamentos);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CriarUser(1, "ADMIN")
            }
        };

        var dto = new CreateSessao
        {
            DataInicio = DateTime.UtcNow.AddDays(1),
            DataFim = DateTime.UtcNow.AddDays(1).AddHours(1),
            InscricaoAberta = true,
            PrecoCoaching = 15
        };

        var result = await controller.Create(dto);

        result.Should().BeOfType<CreatedAtActionResult>();

        db.SessoesAula.Count().Should().Be(1);
    }

    [Fact]
    public async Task TerminarSessao_DeveMarcarComoTerminada()
    {
        var db = DbContextFactory.Create();

        var pagamentos = new PagamentoService(db);

        var sessao = new SessaoAula
        {
            ProfessorUtilizadorId = 1,
            Inicio = DateTime.UtcNow,
            Fim = DateTime.UtcNow.AddHours(1),
            Estado = "AGENDADA",
            CriadoPorUtilizadorId = 1,
            CriadoEm = DateTime.UtcNow,
            FoiDada = false
        };

        db.SessoesAula.Add(sessao);

        await db.SaveChangesAsync();

        var controller = new SessoesController(db, pagamentos);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CriarUser(1, "ADMIN")
            }
        };

        var result = await controller.TerminarAula(sessao.Id);

        result.Should().BeOfType<OkObjectResult>();

        db.SessoesAula.First().FoiDada.Should().BeTrue();
    }
}