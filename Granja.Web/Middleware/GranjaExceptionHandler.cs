using Granja.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Granja.Web.Middleware;
public sealed class GranjaExceptionHandler(ILogger<GranjaExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken ct)
    {
        var status = ex switch
        {
            ValidacionNegocioException => StatusCodes.Status400BadRequest,
            RecursoNoEncontradoException => StatusCodes.Status404NotFound,
            ConflictoNegocioException => StatusCodes.Status409Conflict,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            DbUpdateException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
        if (status == 500) logger.LogError(ex, "Error inesperado");
        else logger.LogInformation("Operación rechazada: {Razon}", ex.Message);
        var titulo = status switch { 400 => "Solicitud inválida", 404 => "Recurso no encontrado", 409 => "Conflicto", _ => "Error interno" };
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status, Title = titulo,
            Detail = status == 500 ? "No se pudo completar la operación." : ex.Message,
            Instance = context.Request.Path.ToString()
        }, options: new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web),
           contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}
