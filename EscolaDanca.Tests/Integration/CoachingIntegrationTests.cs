using EscolaDanca.Models;
using EscolaDanca.Services;
using EscolaDanca.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EscolaDanca.Tests.Integration;

public class CoachingIntegrationTests
{
    [Fact]
    public async Task InscreverAlunoEmCoaching_DeveCriarSessaoAlunoEPagamento()
    {
        var db = DbContextFactory.Create();
        var pagamentoService = new PagamentoService(db);

        var aluno = new Aluno
        {
            Nome = "Aluno Teste",
            Ativo = true
        };

        var sessao = new SessaoAula
        {
            Inicio = DateTime.UtcNow.AddDays(1),
            Fim = DateTime.UtcNow.AddDays(1).AddHours(1),
            Estado = "AGENDADA",
            CriadoPorUtilizadorId = 1,
            CriadoEm = DateTime.UtcNow,
            InscricaoAberta = true,
            FoiDada = false,
            PrecoCoaching = 25
        };

        db.Alunos.Add(aluno);
        db.SessoesAula.Add(sessao);

        await db.SaveChangesAsync();

        db.SessaoAlunos.Add(new SessaoAluno
        {
            SessaoAulaId = sessao.Id,
            AlunoId = aluno.Id,
            AdicionadoPorUtilizadorId = 1,
            AdicionadoEm = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        await pagamentoService.CriarPagamentoCoachingAsync(sessao.Id, aluno.Id);

        var inscricaoExiste = await db.SessaoAlunos
            .AnyAsync(x => x.SessaoAulaId == sessao.Id && x.AlunoId == aluno.Id);

        var pagamento = await db.Pagamentos
            .FirstOrDefaultAsync(p => p.SessaoId == sessao.Id && p.AlunoId == aluno.Id);

        inscricaoExiste.Should().BeTrue();

        pagamento.Should().NotBeNull();
        pagamento!.Tipo.Should().Be("COACHING");
        pagamento.Estado.Should().Be("PENDENTE");
        pagamento.Valor.Should().Be(25);
    }
}