using EscolaDanca.Data;
using EscolaDanca.DTOs;
using EscolaDanca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


[ApiController]
[Route("api/eventos")]
public class EventosController : ControllerBase
{
    private readonly AppDbContext _db;
    public EventosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {


        var query = _db.Eventos.AsQueryable();

      

        var eventos = await query
            .OrderByDescending(e => e.DataInicio)
            .Select(e => new
            {
                e.Id,
                e.Titulo,
                e.Descricao,
                e.DataInicio,
                e.DataFim,
                e.Local,
                e.Publico,
                e.CriadoEm
            })
            .ToListAsync();

        return Ok(eventos);
    }

    // GET /api/eventos/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var perfil = User.FindFirst("perfil")?.Value ?? "";

        var e = await _db.Eventos.FindAsync(id);
        if (e == null) return NotFound();

        if (perfil != "ADMIN" && perfil != "SUPER_ADMIN" && !e.Publico)
            return Forbid();

        return Ok(e);
    }

    // POST /api/eventos
    [HttpPost]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN, PROFESSOR")]
    public async Task<IActionResult> Create([FromBody] EventoRequest req)
    {
        var utilizadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            return Unauthorized();

        var evento = new Evento
        {
            Titulo = req.Titulo,
            Descricao = req.Descricao,
            DataInicio = req.DataInicio,
            DataFim = req.DataFim,
            Local = req.Local,
            Publico = req.Publico,
            CriadoPorUtilizadorId = utilizadorId,
            CriadoEm = DateTime.UtcNow
        };

        _db.Eventos.Add(evento);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = evento.Id }, evento);
    }

    // PUT /api/eventos/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN, PROFESSOR")]
    public async Task<IActionResult> Update(int id, [FromBody] EventoRequest req)
    {
        var evento = await _db.Eventos.FindAsync(id);
        if (evento == null) return NotFound();

        evento.Titulo = req.Titulo;
        evento.Descricao = req.Descricao;
        evento.DataInicio = req.DataInicio;
        evento.DataFim = req.DataFim;
        evento.Local = req.Local;
        evento.Publico = req.Publico;

        await _db.SaveChangesAsync();
        return Ok(evento);
    }

    // DELETE /api/eventos/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN,SUPER_ADMIN, PROFESSOR")]
    public async Task<IActionResult> Delete(int id)
    {
        var evento = await _db.Eventos.FindAsync(id);
        if (evento == null) return NotFound();

        _db.Eventos.Remove(evento);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}