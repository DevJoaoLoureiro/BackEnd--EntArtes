using EscolaDanca.Api.Models;
using EscolaDanca.Models;
using Microsoft.EntityFrameworkCore;

namespace EscolaDanca.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Utilizador> Utilizadores => Set<Utilizador>();
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Responsavel> Responsaveis => Set<Responsavel>();
    public DbSet<AlunoResponsavel> AlunoResponsaveis => Set<AlunoResponsavel>();


    public DbSet<SessaoAula> SessoesAula => Set<SessaoAula>();
    public DbSet<Presenca> Presencas => Set<Presenca>();
    public DbSet<DesmarcacaoAula> DesmarcacoesAula => Set<DesmarcacaoAula>();

    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<Evento> Eventos => Set<Evento>();

    public DbSet<InventarioItem> InventarioItens => Set<InventarioItem>();
    public DbSet<Aluguer> Alugueres => Set<Aluguer>();

    public DbSet<MarketplaceAnuncio> MarketplaceAnuncios => Set<MarketplaceAnuncio>();
    public DbSet<MarketplaceInteresse> MarketplaceInteresses => Set<MarketplaceInteresse>();

    public DbSet<ConfirmacaoPresenca> ConfirmacaoPresenca => Set<ConfirmacaoPresenca>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
}
