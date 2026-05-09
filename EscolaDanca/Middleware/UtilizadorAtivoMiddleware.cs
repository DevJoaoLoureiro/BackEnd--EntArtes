using EscolaDanca.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; // Adiciona esta namespace
using System.Security.Claims;

namespace EscolaDanca.Middleware;

public class UtilizadorAtivoMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache; // Declara o cache

    public UtilizadorAtivoMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache; // Recebe o cache por injeção
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token inválido.");
                return;
            }

            string cacheKey = $"user_ativo_{userId}";

            // Tenta obter o valor do cache. Se não conseguir, entra no IF
            if (!_cache.TryGetValue(cacheKey, out bool userAtivo))
            {
                // VAI À DB (apenas uma vez a cada X minutos)
                userAtivo = await db.Utilizadores
                    .AsNoTracking()
                    .AnyAsync(u => u.Id == userId && u.Ativo);

                // Guarda o resultado no cache por 10 minutos
                _cache.Set(cacheKey, userAtivo, TimeSpan.FromMinutes(10));
            }

            if (!userAtivo)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Utilizador removido ou inativo.");
                return;
            }
        }

        await _next(context);
    }
}
