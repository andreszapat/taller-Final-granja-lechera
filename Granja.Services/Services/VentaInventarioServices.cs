using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
namespace Granja.Services.Services;
public sealed class VentaService(ILoteRepository lotes, IVentaRepository ventas,
    IConfiguracionProductoRepository productos, IUnitOfWork uow, IClock clock)
{
    public async Task<VentaResponse> RegistrarVentaAsync(VentaRequest req, CancellationToken ct = default)
    {
        if (req is null || !Enum.IsDefined(req.TipoProducto) || req.Cantidad <= 0)
            throw new ValidacionNegocioException("La venta requiere un producto válido y una cantidad positiva.");
        var fecha = req.Fecha ?? clock.Today;
        var disponibles = await lotes.ObtenerVendiblesAsync(req.TipoProducto, fecha, ct);
        // Comprobación previa: jamás se descuenta parcialmente una venta sin existencias.
        if (disponibles.Sum(x => x.CantidadDisponible) < req.Cantidad)
            throw new InventarioInsuficienteException(req.TipoProducto.ToString(), req.Cantidad);
        var p = await productos.ObtenerAsync(req.TipoProducto, ct)
            ?? throw new RecursoNoEncontradoException("No existe la configuración del producto.");
        var venta = new Venta(fecha, req.TipoProducto, req.Cantidad, p.PrecioUnitario);
        var restante = req.Cantidad;
        foreach (var lote in disponibles)
        {
            if (restante == 0) break;
            var tomar = Math.Min(lote.CantidadDisponible, restante);
            lote.Descontar(tomar);
            venta.AgregarDetalle(lote.Id, lote.FechaProduccion, tomar);
            restante -= tomar;
        }
        await ventas.AgregarAsync(venta, ct);
        // EF Core confirma la venta y todos los descuentos dentro de una única transacción de SaveChanges.
        await uow.SaveChangesAsync(ct);
        return new(venta.Id, venta.Fecha, venta.Tipo, venta.Cantidad, venta.PrecioUnitario, venta.Total,
            venta.Detalles.Select(d => new VentaDetalleResponse(d.LoteId, d.FechaProduccion, d.Cantidad)).ToList());
    }
}
public sealed class InventarioService(ILoteRepository lotes, IClock clock, IUnitOfWork uow, IMapper mapper)
{
    private InventarioLoteResponse Mapear(LoteProducto lote, DateOnly fecha)
    {
        var dto = mapper.Map<InventarioLoteResponse>(lote);
        dto.CantidadVendible = lote.FechaProduccion <= fecha && !lote.EstaVencido(fecha) ? lote.CantidadDisponible : 0;
        dto.Estado = lote.CantidadDanada > 0 && lote.CantidadDisponible == 0 ? "Danado" :
            lote.CantidadDisponible == 0 ? "Agotado" :
            lote.FechaProduccion > fecha ? "NoProducidoAun" :
            lote.EstaVencido(fecha) ? "VencidoPendiente" : "Disponible";
        return dto;
    }
    public async Task<List<InventarioLoteResponse>> ListarAsync(TipoProducto? tipo = null, DateOnly? fecha = null, CancellationToken ct = default)
    {
        if (tipo.HasValue && !Enum.IsDefined(tipo.Value)) throw new ValidacionNegocioException("Tipo de producto no válido.");
        var f = fecha ?? clock.Today;
        return (await lotes.ListarAsync(ct)).Where(x => !tipo.HasValue || x.Tipo == tipo.Value)
            .Select(x => Mapear(x, f)).ToList();
    }
    public async Task<VencimientosResponse> ProcesarVencimientosAsync(VencimientoRequest? req = null, CancellationToken ct = default)
    {
        var fecha = req?.Fecha ?? clock.Today;
        var vencidos = await lotes.ObtenerVencidosAsync(fecha, ct);
        var danados = vencidos.Sum(x => x.MarcarComoDanado(fecha));
        await uow.SaveChangesAsync(ct);
        return new(fecha, vencidos.Count, danados, vencidos.Select(x => Mapear(x, fecha)).ToList());
    }
}
