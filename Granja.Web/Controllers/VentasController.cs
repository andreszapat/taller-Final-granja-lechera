using Granja.Services.Dtos;
using Granja.Services.Services;
using Microsoft.AspNetCore.Mvc;
namespace Granja.Web.Controllers;
[ApiController, Route("api/ventas")]
public sealed class VentasController(VentaService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<VentaResponse>> Registrar([FromBody] VentaRequest req, CancellationToken ct)
        => StatusCode(201, await service.RegistrarVentaAsync(req, ct));
}
