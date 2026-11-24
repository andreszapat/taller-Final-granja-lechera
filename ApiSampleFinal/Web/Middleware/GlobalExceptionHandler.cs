using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ApiSampleFinal.Middleware
{
    /// <summary>Traduce las excepciones a respuestas ProblemDetails con el código HTTP adecuado.</summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetails = problemDetails;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
        {
            var (status, title) = exception switch
            {
                EntidadNoEncontradaException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                ConflictoException => (StatusCodes.Status409Conflict, "Conflicto con el estado actual"),
                DomainException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
                InvalidOperationException => (StatusCodes.Status400BadRequest, "Operación inválida"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Error no controlado");

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "Ocurrió un error inesperado."
                    : exception.Message,
                Instance = httpContext.Request.Path
            };
            problem.Extensions["tipoError"] = exception.GetType().Name;

            httpContext.Response.StatusCode = status;
            return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });
        }
    }
}
