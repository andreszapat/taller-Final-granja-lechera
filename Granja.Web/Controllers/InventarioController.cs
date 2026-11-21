using Granja.Domain.Enums;
using Granja.Services.Dtos;
using Granja.Services.Services;
using Microsoft.AspNetCore.Mvc;
namespace Granja.Web.Controllers;
[ApiController, Route("api/inventario")]
public sealed class InventarioController(InventarioService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<InventarioLoteResponse>>> Listar([FromQuery] TipoProducto? tipo,
        [FromQuery] DateOnly? fecha, CancellationToken ct)
        => Ok(await service.ListarAsync(tipo, fecha, ct));
    [HttpPost("procesar-vencimientos")]
    public async Task<ActionResult<VencimientosResponse>> Procesar([FromBody] VencimientoRequest? req, CancellationToken ct)
        => Ok(await service.ProcesarVencimientosAsync(req, ct));
}
