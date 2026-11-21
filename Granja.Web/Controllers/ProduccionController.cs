using Granja.Services.Dtos;
using Granja.Services.Services;
using Microsoft.AspNetCore.Mvc;
namespace Granja.Web.Controllers;
[ApiController, Route("api/produccion")]
public sealed class ProduccionController(ProduccionService service) : ControllerBase
{
    [HttpPost("leche")]
    public async Task<ActionResult<ProduccionResponse>> Registrar([FromBody] ProduccionRequest req, CancellationToken ct)
        => StatusCode(201, await service.RegistrarLecheAsync(req, ct));
    [HttpPost("transformaciones")]
    public async Task<ActionResult<TransformacionResponse>> Transformar([FromBody] TransformacionRequest req, CancellationToken ct)
        => StatusCode(201, await service.TransformarAsync(req, ct));
}
