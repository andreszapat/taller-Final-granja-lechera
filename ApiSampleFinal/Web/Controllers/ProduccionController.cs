using Microsoft.AspNetCore.Mvc;
using Services.Dtos;
using Services.Interfaces;

namespace ApiSampleFinal.Controllers
{
    [ApiController]
    [Route("api/produccion")]
    [Produces("application/json")]
    public class ProduccionController : ControllerBase
    {
        private readonly IProduccionService _service;
        public ProduccionController(IProduccionService service) => _service = service;

        /// <summary>Registra la producción del día de todas las vacas activas (n × l). Una sola vez por fecha.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ProduccionDiaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProduccionDiaResponse>> RegistrarDia([FromBody] RegistrarProduccionRequest request, CancellationToken ct)
        {
            var dia = await _service.RegistrarDiaAsync(request, ct);
            return CreatedAtAction(nameof(ObtenerDia), new { fecha = dia.Fecha.ToString("yyyy-MM-dd") }, dia);
        }

        /// <summary>Registra la producción de una sola vaca en una fecha.</summary>
        [HttpPost("vaca")]
        [ProducesResponseType(typeof(ProduccionDiaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProduccionDiaResponse>> RegistrarVaca([FromBody] RegistrarProduccionVacaRequest request, CancellationToken ct)
        {
            var dia = await _service.RegistrarVacaAsync(request, ct);
            return CreatedAtAction(nameof(ObtenerDia), new { fecha = dia.Fecha.ToString("yyyy-MM-dd") }, dia);
        }

        /// <summary>Lista la producción por día en un rango de fechas.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<ProduccionDiaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ProduccionDiaResponse>>> Listar([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
            Ok(await _service.ListarAsync(desde, hasta, ct));

        /// <summary>Producción de una fecha con el detalle por vaca y los litros aún por transformar.</summary>
        [HttpGet("{fecha}")]
        [ProducesResponseType(typeof(ProduccionDiaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProduccionDiaResponse>> ObtenerDia(DateOnly fecha, CancellationToken ct) =>
            Ok(await _service.ObtenerDiaAsync(fecha, ct));

        /// <summary>Transforma litros de leche de una fecha en queso o yogurt y crea un lote en el inventario.</summary>
        [HttpPost("transformaciones")]
        [ProducesResponseType(typeof(LoteResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LoteResponse>> Transformar([FromBody] TransformacionRequest request, CancellationToken ct)
        {
            var lote = await _service.TransformarAsync(request, ct);
            return CreatedAtAction(nameof(InventarioController.ObtenerLote), "Inventario", new { id = lote.Id }, lote);
        }
    }
}
