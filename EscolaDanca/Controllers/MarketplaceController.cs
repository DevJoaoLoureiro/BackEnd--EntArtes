using EscolaDanca.Data;
using EscolaDanca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/marketplace")]
public class MarketplaceController : ControllerBase
{
    private readonly AppDbContext _db;

    public MarketplaceController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var anuncios = await _db.MarketplaceAnuncios
            .Where(a => a.Estado == "APROVADO" && a.Ativo)
            .OrderByDescending(a => a.CriadoEm)
            .ToListAsync();

        return Ok(anuncios);
    }

    [HttpPost("anuncios")]
    [Authorize(Roles = "ENCARREGADO,ALUNO")]
    public async Task<IActionResult> CriarAnuncio([FromBody] MarketplaceAnuncio req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        req.DonoUtilizadorId = userId;
        req.CriadoEm = DateTime.UtcNow;
        req.Estado = "PENDENTE";

        _db.MarketplaceAnuncios.Add(req);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Anúncio criado. Aguarda aprovação." });
    }

    [HttpPatch("anuncios/{id}/aprovar")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Aprovar(int id)
    {
        var anuncio = await _db.MarketplaceAnuncios.FindAsync(id);
        if (anuncio == null)
            return NotFound();

        anuncio.Estado = "APROVADO";
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("anuncios/{id}/alugar")]
    [Authorize(Roles = "ENCARREGADO,ALUNO")]
    public async Task<IActionResult> Alugar(int id, [FromBody] int alunoId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var anuncio = await _db.MarketplaceAnuncios.FindAsync(id);
        if (anuncio == null || anuncio.Estado != "APROVADO")
            return BadRequest("Anúncio inválido.");

        var jaAlugado = await _db.MarketplaceAlugueres
            .AnyAsync(a => a.AnuncioId == id && a.AlunoId == alunoId && a.Estado == "ATIVO");

        if (jaAlugado)
            return BadRequest("Já alugado.");

        var aluguer = new MarketplaceAluguer
        {
            AnuncioId = id,
            AlunoId = alunoId,
            AlugadoPorUtilizadorId = userId,
            DataInicio = DateTime.UtcNow,
            Estado = "ATIVO",
            Valor = anuncio.PrecoAluguer
        };

        _db.MarketplaceAlugueres.Add(aluguer);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Alugado com sucesso." });
    }

    [HttpPatch("alugueres/{id}/devolver")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Devolver(int id)
    {
        var aluguer = await _db.MarketplaceAlugueres.FindAsync(id);
        if (aluguer == null)
            return NotFound();

        aluguer.Estado = "DEVOLVIDO";
        aluguer.DataDevolucao = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok();
    }
}