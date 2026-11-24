using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Services.Dtos;
using Services.Interfaces;

namespace ApiSampleFinal.Controllers
{
    [ApiController]
    [Route("api/inventario")]
    [Produces("application/json")]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioService _service;
        public InventarioController(IInventarioService service) => _service = service;

        /// <summary>Consulta el inventario a una fecha: existencias vendibles por producto y lotes.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(InventarioResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<InventarioResponse>> Consultar([FromQuery] TipoProducto? tipo, [FromQuery] DateOnly? fecha,
            [FromQuery] bool incluirAgotados = false, CancellationToken ct = default) =>
            Ok(await _service.ConsultarAsync(tipo, fecha, incluirAgotados, ct));

        /// <summary>Consulta un lote.</summary>
        [HttpGet("lotes/{id:int}")]
        [ProducesResponseType(typeof(LoteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoteResponse>> ObtenerLote(int id, [FromQuery] DateOnly? fecha, CancellationToken ct) =>
            Ok(await _service.ObtenerLoteAsync(id, fecha, ct));

        /// <summary>Procesa vencimientos: la cantidad disponible de los lotes vencidos pasa a producto dañado.</summary>
        [HttpPost("procesar-vencimientos")]
        [ProducesResponseType(typeof(ProcesarVencimientosResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProcesarVencimientosResponse>> ProcesarVencimientos([FromBody] ProcesarVencimientosRequest? request, CancellationToken ct) =>
            Ok(await _service.ProcesarVencimientosAsync(request?.Fecha, ct));

        /// <summary>Alerta de lotes con existencias que vencen en los próximos días (por defecto 3).</summary>
        [HttpGet("proximos-a-vencer")]
        [ProducesResponseType(typeof(IReadOnlyList<LoteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IReadOnlyList<LoteResponse>>> ProximosAVencer([FromQuery] int dias = 3, [FromQuery] DateOnly? fecha = null, CancellationToken ct = default) =>
            Ok(await _service.ProximosAVencerAsync(dias, fecha, ct));
    }

    [ApiController]
    [Route("api/ventas")]
    [Produces("application/json")]
    public class VentasController : ControllerBase
    {
        private readonly IVentaService _service;
        public VentasController(IVentaService service) => _service = service;

        /// <summary>Registra una venta. Despacha FIFO el producto no vencido más antiguo; 409 si no hay existencias.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(VentaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<VentaResponse>> Registrar([FromBody] VentaRequest request, CancellationToken ct)
        {
            var venta = await _service.RegistrarVentaAsync(request, ct);
            return CreatedAtAction(nameof(Obtener), new { id = venta.Id }, venta);
        }

        /// <summary>Consulta una venta con el detalle de lotes.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VentaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VentaResponse>> Obtener(int id, CancellationToken ct) =>
            Ok(await _service.ObtenerAsync(id, ct));

        /// <summary>Lista ventas filtradas por fechas y producto.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<VentaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<VentaResponse>>> Listar([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
            [FromQuery] TipoProducto? tipo, CancellationToken ct) =>
            Ok(await _service.ListarAsync(desde, hasta, tipo, ct));
    }
}
