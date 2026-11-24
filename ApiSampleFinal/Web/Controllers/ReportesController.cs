using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Services.Dtos;
using Services.Interfaces;

namespace ApiSampleFinal.Controllers
{
    [ApiController]
    [Route("api/reportes")]
    [Produces("application/json")]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _service;
        public ReportesController(IReporteService service) => _service = service;

        /// <summary>Ventas del período: fecha, producto, cantidad, precio, total y total general.</summary>
        [HttpGet("ventas")]
        [ProducesResponseType(typeof(ReporteVentasResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReporteVentasResponse>> Ventas([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
            [FromQuery] TipoProducto? tipo, CancellationToken ct) =>
            Ok(await _service.VentasAsync(desde, hasta, tipo, ct));

        /// <summary>Leche total producida por día, litros transformados y litros no aprovechados.</summary>
        [HttpGet("leche-diaria")]
        [ProducesResponseType(typeof(ReporteLecheDiariaResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReporteLecheDiariaResponse>> LecheDiaria([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
            Ok(await _service.LecheDiariaAsync(desde, hasta, ct));

        /// <summary>Producto elaborado por día y por tipo (queso y yogurt).</summary>
        [HttpGet("producto-diario")]
        [ProducesResponseType(typeof(IReadOnlyList<ReporteProductoDiarioItem>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ReporteProductoDiarioItem>>> ProductoDiario([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
            Ok(await _service.ProductoDiarioAsync(desde, hasta, ct));

        /// <summary>Por producto: total producido, vendido, disponible y dañado.</summary>
        [HttpGet("producto-total")]
        [ProducesResponseType(typeof(IReadOnlyList<ReporteProductoTotalItem>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ReporteProductoTotalItem>>> ProductoTotal(CancellationToken ct) =>
            Ok(await _service.ProductoTotalAsync(ct));

        /// <summary>Producto dañado por lote y por tipo, con fecha de vencimiento (filtra por vencimiento).</summary>
        [HttpGet("producto-danado")]
        [ProducesResponseType(typeof(ReporteProductoDanadoResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReporteProductoDanadoResponse>> ProductoDanado([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
            Ok(await _service.ProductoDanadoAsync(desde, hasta, ct));

        /// <summary>Producción por vaca: promedio, mayor y menor productora y vacas bajo el promedio.</summary>
        [HttpGet("vacas")]
        [ProducesResponseType(typeof(ReporteVacasResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReporteVacasResponse>> Vacas([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
            Ok(await _service.VacasAsync(desde, hasta, ct));
    }
}
