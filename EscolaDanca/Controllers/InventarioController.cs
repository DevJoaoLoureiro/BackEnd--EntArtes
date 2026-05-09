using EscolaDanca.Data;
using EscolaDanca.DTOs;
using EscolaDanca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/inventario")]
public class InventarioController : ControllerBase
{
    private readonly AppDbContext _db;

    public InventarioController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.InventarioItens
            .Where(i => i.Ativo)
            .OrderBy(i => i.Nome)
            .ToListAsync();

        return Ok(items);
    }



    [HttpPost]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Create([FromForm] CreateInventarioItemRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return BadRequest("Nome é obrigatório.");

        if (req.QuantidadeDisponivel > req.QuantidadeTotal)
            return BadRequest("Quantidade disponível não pode ser maior que a total.");

        string? imagemUrl = null;

        if (req.Imagem != null && req.Imagem.Length > 0)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(req.Imagem.FileName).ToLowerInvariant();

            if (!allowed.Contains(ext))
                return BadRequest("Formato de imagem inválido. Usa jpg, png ou webp.");

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "inventario");

            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await req.Imagem.CopyToAsync(stream);
            }

            imagemUrl = $"/uploads/inventario/{fileName}";
        }

        var item = new InventarioItem
        {
            Nome = req.Nome.Trim(),
            Categoria = string.IsNullOrWhiteSpace(req.Categoria) ? null : req.Categoria.Trim(),
            Tamanho = string.IsNullOrWhiteSpace(req.Tamanho) ? null : req.Tamanho.Trim(),
            QuantidadeTotal = req.QuantidadeTotal,
            QuantidadeDisponivel = req.QuantidadeDisponivel,
            PrecoAluguer = req.PrecoAluguer,
            Localizacao = string.IsNullOrWhiteSpace(req.Localizacao) ? null : req.Localizacao.Trim(),
            ImagemUrl = imagemUrl,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _db.InventarioItens.Add(item);
        await _db.SaveChangesAsync();

        return Ok(item);
    }


    [HttpGet("me")]
    [Authorize(Roles = "ALUNO")]
    public async Task<IActionResult> GetMe()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var aluno = await _db.Alunos
            .Where(a => a.UtilizadorId == userId)
            .Select(a => new { a.Id, a.Nome })
            .FirstOrDefaultAsync();

        if (aluno == null)
            return NotFound();

        return Ok(aluno);
    }


    [HttpPost("{id}/alugar")]
    [Authorize(Roles = "ENCARREGADO,ALUNO")]
    public async Task<IActionResult> Alugar(int id, [FromBody] int alunoId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var item = await _db.InventarioItens.FindAsync(id);
        if (item == null)
            return NotFound("Item não encontrado.");

        if (item.QuantidadeDisponivel <= 0)
            return BadRequest("Sem stock disponível.");

        var jaAlugado = await _db.InventarioMovimentos
            .AnyAsync(m => m.ItemId == id && m.AlunoId == alunoId && m.Estado == "ATIVO");

        if (jaAlugado)
            return BadRequest("Este aluno já tem este item alugado.");

        var movimento = new InventarioMovimento
        {
            ItemId = id,
            Tipo = "ALUGUER",
            Quantidade = 1,
            AlunoId = alunoId,
            CriadoPorUtilizadorId = userId,
            CriadoEm = DateTime.UtcNow,
            Estado = "ATIVO",
            Valor = item.PrecoAluguer
        };

        item.QuantidadeDisponivel -= 1;

        _db.InventarioMovimentos.Add(movimento);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Item alugado com sucesso." });
    }

    [HttpPatch("movimentos/{id}/devolver")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> Devolver(int id)
    {
        var movimento = await _db.InventarioMovimentos
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movimento == null)
            return NotFound();

        if (movimento.Estado != "ATIVO")
            return BadRequest("Já foi devolvido.");

        var item = await _db.InventarioItens.FindAsync(movimento.ItemId);

        movimento.Estado = "DEVOLVIDO";
        movimento.DataDevolucao = DateTime.UtcNow;

        if (item != null)
            item.QuantidadeDisponivel += movimento.Quantidade;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Devolvido com sucesso." });
    }
}