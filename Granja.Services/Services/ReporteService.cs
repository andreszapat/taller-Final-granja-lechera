using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
namespace Granja.Services.Services;
public sealed class ReporteService(IProduccionRepository producciones, ITransformacionRepository transformaciones,
    ILoteRepository lotes, IVentaRepository ventas, IVacaRepository vacas, IClock clock)
{
    private static void ValidarFechas(DateOnly? desde, DateOnly? hasta)
    {
        if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            throw new ValidacionNegocioException("La fecha 'desde' no puede ser posterior a 'hasta'.");
    }
    public async Task<VentasReporteResponse> VentasAsync(DateOnly? desde = null, DateOnly? hasta = null,
        TipoProducto? tipo = null, CancellationToken ct = default)
    {
        ValidarFechas(desde, hasta);
        if (tipo.HasValue && !Enum.IsDefined(tipo.Value)) throw new ValidacionNegocioException("Producto no válido.");
        var datos = await ventas.ListarAsync(desde, hasta, tipo, ct);
        return new(datos.Select(x => new VentasReporteItem(x.Id, x.Fecha, x.Tipo, x.Cantidad, x.PrecioUnitario, x.Total)).ToList(),
            datos.Sum(x => x.Total));
    }
    public async Task<List<LecheDiariaItem>> LecheDiariaAsync(DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default)
    {
        ValidarFechas(desde, hasta);
        var registros = await producciones.ListarAsync(desde, hasta, ct);
        var usos = await transformaciones.ListarAsync(desde, hasta, ct);
        return registros.GroupBy(x => x.Fecha).OrderBy(g => g.Key).Select(g =>
        {
            var producido = g.Sum(x => x.Litros);
            var transformado = usos.Where(t => t.Fecha == g.Key).Sum(t => t.LitrosUsados);
            return new LecheDiariaItem(g.Key, producido, transformado, producido - transformado);
        }).ToList();
    }
    public async Task<List<ProductoDiarioItem>> ProductoDiarioAsync(DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default)
    {
        ValidarFechas(desde, hasta);
        var datos = await transformaciones.ListarAsync(desde, hasta, ct);
        return datos.GroupBy(x => new { x.Fecha, x.Tipo }).OrderBy(g => g.Key.Fecha).ThenBy(g => g.Key.Tipo)
            .Select(g => new ProductoDiarioItem(g.Key.Fecha, g.Key.Tipo,
                g.Sum(x => x.CantidadObtenida), g.Sum(x => x.LitrosUsados))).ToList();
    }
    public async Task<List<ProductoTotalItem>> ProductoTotalAsync(CancellationToken ct = default)
    {
        var todosLotes = await lotes.ListarAsync(ct);
        var todasVentas = await ventas.ListarAsync(ct: ct);
        var hoy = clock.Today;
        return Enum.GetValues<TipoProducto>().Select(tipo =>
        {
            var productos = todosLotes.Where(x => x.Tipo == tipo).ToList();
            return new ProductoTotalItem(tipo, productos.Sum(x => x.CantidadInicial),
                todasVentas.Where(x => x.Tipo == tipo).Sum(x => x.Cantidad),
                productos.Sum(x => x.CantidadDisponible), productos.Sum(x => x.CantidadDanada),
                productos.Where(x => x.EstaVencido(hoy)).Sum(x => x.CantidadDisponible));
        }).ToList();
    }
    public async Task<List<ProductoDanadoItem>> ProductoDanadoAsync(DateOnly? desde = null, DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        ValidarFechas(desde, hasta);
        // Los filtros se aplican sobre la fecha de VENCIMIENTO, no sobre la de procesamiento.
        return (await lotes.ListarAsync(ct)).Where(x => x.CantidadDanada > 0)
            .Where(x => !desde.HasValue || x.FechaVencimiento >= desde.Value)
            .Where(x => !hasta.HasValue || x.FechaVencimiento <= hasta.Value)
            .Select(x => new ProductoDanadoItem(x.Id, x.Tipo, x.FechaProduccion,
                x.FechaVencimiento, x.FechaProcesoDanado, x.CantidadDanada)).ToList();
    }
    public async Task<ReporteVacasResponse> VacasAsync(DateOnly? desde = null, DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        ValidarFechas(desde, hasta);
        var producidas = await producciones.ListarAsync(desde, hasta, ct);
        var catalogo = await vacas.ListarAsync(ct);
        var vacasResumen = catalogo.Select(v =>
        {
            var r = producidas.Where(x => x.VacaId == v.Id).ToList();
            var total = r.Sum(x => x.Litros);
            var dias = r.Select(x => x.Fecha).Distinct().Count();
            return new ProduccionVacaItem(v.Id, v.Codigo, v.Nombre, total, dias, dias == 0 ? 0 : total / dias);
        }).Where(x => x.DiasRegistrados > 0).OrderBy(x => x.VacaId).ToList();
        if (vacasResumen.Count == 0) return new(0, vacasResumen, new(), new(), new());
        var promedio = vacasResumen.Average(x => x.PromedioLitrosPorDia);
        var max = vacasResumen.Max(x => x.PromedioLitrosPorDia);
        var min = vacasResumen.Min(x => x.PromedioLitrosPorDia);
        return new(promedio, vacasResumen,
            vacasResumen.Where(x => x.PromedioLitrosPorDia == max).ToList(),
            vacasResumen.Where(x => x.PromedioLitrosPorDia == min).ToList(),
            vacasResumen.Where(x => x.PromedioLitrosPorDia < promedio).ToList());
    }
}
