using EscolaDanca.Data;
using EscolaDanca.Models;
using Microsoft.EntityFrameworkCore;

namespace EscolaDanca.Services;

public class PagamentoService
{
    private readonly AppDbContext _db;

    public PagamentoService(AppDbContext db)
    {
        _db = db;
    }

    // =========================
    // COACHING
    // =========================
    public async Task CriarPagamentoCoachingAsync(int sessaoId, int alunoId)
    {
        var sessao = await _db.SessoesAula
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessaoId);

        if (sessao == null)
            return;

        // Só coaching
        if (!sessao.InscricaoAberta)
            return;

        if (!sessao.PrecoCoaching.HasValue || sessao.PrecoCoaching.Value <= 0)
            return;

        var jaExiste = await _db.Pagamentos.AnyAsync(p =>
            p.AlunoId == alunoId &&
            p.Tipo == "COACHING" &&
            p.SessaoId == sessaoId);

        if (jaExiste)
            return;

        var pagamento = new Pagamento
        {
            AlunoId = alunoId,
            SessaoId = sessaoId,
            Tipo = "COACHING",
            Valor = sessao.PrecoCoaching.Value,
            Estado = "PENDENTE",
            Referencia = $"COACH-S{sessaoId}-A{alunoId}",
            Descricao = $"Coaching #{sessaoId}",
            CriadoEm = DateTime.UtcNow
        };

        _db.Pagamentos.Add(pagamento);

        await _db.SaveChangesAsync();
    }

    // =========================
    // MENSALIDADE
    // =========================
    public async Task CriarPagamentoMensalidadeAsync(int turmaId, int alunoId)
    {
        var turma = await _db.Turmas
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == turmaId);

        if (turma == null)
            return;

        if (!turma.PrecoMensal.HasValue || turma.PrecoMensal.Value <= 0)
            return;

        var hoje = DateTime.Today;

        var jaExiste = await _db.Pagamentos.AnyAsync(p =>
            p.AlunoId == alunoId &&
            p.Tipo == "MENSALIDADE" &&
            p.Mes == hoje.Month &&
            p.Ano == hoje.Year);

        if (jaExiste)
            return;

        var pagamento = new Pagamento
        {
            AlunoId = alunoId,
            Tipo = "MENSALIDADE",
            Valor = turma.PrecoMensal.Value,
            Estado = "PENDENTE",
            Mes = hoje.Month,
            Ano = hoje.Year,
            Referencia = $"MENS-{hoje.Year}-{hoje.Month:00}-T{turmaId}-A{alunoId}",
            Descricao = $"Mensalidade {hoje.Month:00}/{hoje.Year}",
            CriadoEm = DateTime.UtcNow
        };

        _db.Pagamentos.Add(pagamento);

        await _db.SaveChangesAsync();
    }
}