using Granja.Services.Dtos;
using Granja.Services.Services;
using Microsoft.AspNetCore.Mvc;
namespace Granja.Web.Controllers;
[ApiController, Route("api/granja")]
public sealed class GranjaController(GranjaService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<GranjaConfigResponse>> Obtener(CancellationToken ct)
        => Ok(await service.ObtenerAsync(ct));
    [HttpPut] public async Task<ActionResult<GranjaConfigResponse>> Actualizar([FromBody] GranjaActualizarRequest req, CancellationToken ct)
        => Ok(await service.ActualizarAsync(req, ct));
}
[ApiController, Route("api/vacas")]
public sealed class VacasController(VacaService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<List<VacaResponse>>> Listar(CancellationToken ct)
        => Ok(await service.ListarAsync(ct));
    [HttpPut("{id:int}")] public async Task<ActionResult<VacaResponse>> Renombrar(int id, [FromBody] RenombrarVacaRequest req, CancellationToken ct)
        => Ok(await service.RenombrarAsync(id, req, ct));
}
