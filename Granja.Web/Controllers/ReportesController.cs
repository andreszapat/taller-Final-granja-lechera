using Granja.Domain.Enums;
using Granja.Services.Dtos;
using Granja.Services.Services;
using Microsoft.AspNetCore.Mvc;
namespace Granja.Web.Controllers;
[ApiController, Route("api/reportes")]
public sealed class ReportesController(ReporteService service) : ControllerBase
{
    [HttpGet("ventas")]
    public async Task<ActionResult<VentasReporteResponse>> Ventas([FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta, [FromQuery] TipoProducto? tipo, CancellationToken ct)
        => Ok(await service.VentasAsync(desde, hasta, tipo, ct));
    [HttpGet("leche-diaria")]
    public async Task<ActionResult<List<LecheDiariaItem>>> LecheDiaria([FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta, CancellationToken ct)
        => Ok(await service.LecheDiariaAsync(desde, hasta, ct));
    [HttpGet("producto-diario")]
    public async Task<ActionResult<List<ProductoDiarioItem>>> ProductoDiario([FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta, CancellationToken ct)
        => Ok(await service.ProductoDiarioAsync(desde, hasta, ct));
    [HttpGet("producto-total")]
    public async Task<ActionResult<List<ProductoTotalItem>>> ProductoTotal(CancellationToken ct)
        => Ok(await service.ProductoTotalAsync(ct));
    [HttpGet("producto-danado")]
    public async Task<ActionResult<List<ProductoDanadoItem>>> ProductoDanado([FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta, CancellationToken ct)
        => Ok(await service.ProductoDanadoAsync(desde, hasta, ct));
    [HttpGet("vacas")]
    public async Task<ActionResult<ReporteVacasResponse>> Vacas([FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta, CancellationToken ct)
        => Ok(await service.VacasAsync(desde, hasta, ct));
}
