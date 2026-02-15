using EscolaDanca.Data;
using EscolaDanca.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaDanca.Api.Controllers;

[ApiController]
[Route("api/alunos")]
public class AlunosController : ControllerBase
{
    private readonly AppDbContext _db;
    public AlunosController(AppDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> Listar()
    {
        var alunos = await _db.Alunos.OrderBy(a => a.Nome).ToListAsync();
        return Ok(alunos);
    }

    [HttpGet("count")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN,PROFESSOR")]
    public async Task<IActionResult> Count()
    {
        var total = await _db.Alunos.CountAsync();
        return Ok(new { total });
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Criar(Aluno aluno)
    {
        _db.Alunos.Add(aluno);
        await _db.SaveChangesAsync();
        return Ok(aluno);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Remover(int id)
    {
        var aluno = await _db.Alunos.FindAsync(id);
        if (aluno == null) return NotFound();

        _db.Alunos.Remove(aluno);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
