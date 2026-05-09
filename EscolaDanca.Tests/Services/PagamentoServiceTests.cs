using EscolaDanca.Models;
using EscolaDanca.Services;
using EscolaDanca.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace EscolaDanca.Tests.Services;

public class PagamentoServiceTests
{
    [Fact]
    public async Task DeveCriarPagamentoCoaching()
    {
        var db = DbContextFactory.Create();

        var aluno = new Aluno
        {
            Nome = "Teste"
        };

        db.Alunos.Add(aluno);

        var sessao = new SessaoAula
        {
            Inicio = DateTime.UtcNow,
            Fim = DateTime.UtcNow.AddHours(1),
            Estado = "AGENDADA",
            CriadoPorUtilizadorId = 1,
            CriadoEm = DateTime.UtcNow,
            InscricaoAberta = true,
            PrecoCoaching = 20
        };

        db.SessoesAula.Add(sessao);

        await db.SaveChangesAsync();

        var service = new PagamentoService(db);

        await service.CriarPagamentoCoachingAsync(aluno.Id, sessao.Id);

        db.Pagamentos.Count().Should().Be(1);

        var pagamento = db.Pagamentos.First();

        pagamento.Valor.Should().Be(20);
        pagamento.Estado.Should().Be("PENDENTE");
    }
}