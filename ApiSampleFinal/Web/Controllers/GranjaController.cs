using Microsoft.AspNetCore.Mvc;
using Services.Dtos;
using Services.Interfaces;

namespace ApiSampleFinal.Controllers
{
    [ApiController]
    [Route("api/granja")]
    [Produces("application/json")]
    public class GranjaController : ControllerBase
    {
        private readonly IGranjaService _service;
        public GranjaController(IGranjaService service) => _service = service;

        /// <summary>Consulta la configuración (n, l) y los parámetros por producto.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(GranjaResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GranjaResponse>> Obtener(CancellationToken ct) =>
            Ok(await _service.ObtenerAsync(ct));

        /// <summary>Actualiza n, l y los parámetros de rendimiento, vida útil y precio. Ajusta las vacas activas a n.</summary>
        [HttpPut]
        [ProducesResponseType(typeof(GranjaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GranjaResponse>> Actualizar([FromBody] ActualizarGranjaRequest request, CancellationToken ct) =>
            Ok(await _service.ActualizarAsync(request, ct));
    }

    [ApiController]
    [Route("api/vacas")]
    [Produces("application/json")]
    public class VacasController : ControllerBase
    {
        private readonly IVacaService _service;
        public VacasController(IVacaService service) => _service = service;

        /// <summary>Lista las vacas de la granja.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<VacaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<VacaResponse>>> Listar(CancellationToken ct) =>
            Ok(await _service.ListarAsync(ct));

        /// <summary>Consulta una vaca.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VacaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VacaResponse>> Obtener(int id, CancellationToken ct) =>
            Ok(await _service.ObtenerAsync(id, ct));

        /// <summary>Agrega una vaca activa (n aumenta en 1).</summary>
        [HttpPost]
        [ProducesResponseType(typeof(VacaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<VacaResponse>> Crear([FromBody] CrearVacaRequest request, CancellationToken ct)
        {
            var vaca = await _service.CrearAsync(request, ct);
            return CreatedAtAction(nameof(Obtener), new { id = vaca.Id }, vaca);
        }
    }
}
