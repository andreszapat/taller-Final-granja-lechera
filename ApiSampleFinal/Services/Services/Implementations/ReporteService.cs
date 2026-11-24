using AutoMapper;
using Domain.Enums;
using Domain.Interfaces;
using Services.Dtos;
using Services.Interfaces;

namespace Services.Implementations
{
    public class ReporteService : IReporteService
    {
        private readonly IProduccionLecheRepository _producciones;
        private readonly ILoteRepository _lotes;
        private readonly IVentaRepository _ventas;
        private readonly IGranjaRepository _granjas;
        private readonly IMapper _mapper;

        public ReporteService(IProduccionLecheRepository producciones, ILoteRepository lotes, IVentaRepository ventas,
            IGranjaRepository granjas, IMapper mapper)
        {
            _producciones = producciones;
            _lotes = lotes;
            _ventas = ventas;
            _granjas = granjas;
            _mapper = mapper;
        }

        public async Task<ReporteVentasResponse> VentasAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default)
        {
            Validaciones.ValidarRango(desde, hasta);
            var ventas = await _ventas.ObtenerPorRangoAsync(desde, hasta, tipo, ct);
            return new ReporteVentasResponse
            {
                Desde = desde,
                Hasta = hasta,
                Tipo = tipo,
                Ventas = _mapper.Map<List<ReporteVentaItem>>(ventas),
                CantidadTotal = ventas.Sum(v => v.Cantidad),
                TotalGeneral = ventas.Sum(v => v.Total)
            };
        }

        public async Task<ReporteLecheDiariaResponse> LecheDiariaAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
        {
            Validaciones.ValidarRango(desde, hasta);
            var producciones = await _producciones.ObtenerPorRangoAsync(desde, hasta, ct);
            var lotes = await _lotes.ObtenerPorRangoAsync(desde, hasta, null, ct);
            var transformados = lotes.GroupBy(l => l.FechaProduccion).ToDictionary(g => g.Key, g => g.Sum(l => l.LitrosUsados));

            var dias = producciones
                .GroupBy(p => p.Fecha)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var producidos = g.Sum(p => p.Litros);
                    var usados = transformados.GetValueOrDefault(g.Key);
                    return new ReporteLecheDiariaItem
                    {
                        Fecha = g.Key,
                        VacasOrdenadas = g.Count(),
                        LitrosProducidos = producidos,
                        LitrosTransformados = usados,
                        LitrosNoAprovechados = producidos - usados
                    };
                })
                .ToList();

            return new ReporteLecheDiariaResponse
            {
                Dias = dias,
                TotalProducido = dias.Sum(d => d.LitrosProducidos),
                TotalTransformado = dias.Sum(d => d.LitrosTransformados),
                TotalNoAprovechado = dias.Sum(d => d.LitrosNoAprovechados)
            };
        }

        public async Task<IReadOnlyList<ReporteProductoDiarioItem>> ProductoDiarioAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
        {
            Validaciones.ValidarRango(desde, hasta);
            var unidades = await UnidadesAsync(ct);
            var lotes = await _lotes.ObtenerPorRangoAsync(desde, hasta, null, ct);
            return lotes
                .GroupBy(l => new { l.FechaProduccion, l.Tipo })
                .OrderBy(g => g.Key.FechaProduccion).ThenBy(g => g.Key.Tipo)
                .Select(g => new ReporteProductoDiarioItem
                {
                    Fecha = g.Key.FechaProduccion,
                    TipoProducto = g.Key.Tipo,
                    Unidad = unidades.GetValueOrDefault(g.Key.Tipo, string.Empty),
                    Lotes = g.Count(),
                    LitrosUsados = g.Sum(l => l.LitrosUsados),
                    Cantidad = g.Sum(l => l.CantidadInicial)
                })
                .ToList();
        }

        public async Task<IReadOnlyList<ReporteProductoTotalItem>> ProductoTotalAsync(CancellationToken ct = default)
        {
            var parametros = await _granjas.ObtenerParametrosAsync(ct);
            var lotes = await _lotes.ObtenerPorRangoAsync(null, null, null, ct);
            var ventas = await _ventas.ObtenerPorRangoAsync(null, null, null, ct);

            return parametros.Select(p =>
            {
                var delTipo = lotes.Where(l => l.Tipo == p.Tipo).ToList();
                return new ReporteProductoTotalItem
                {
                    TipoProducto = p.Tipo,
                    Unidad = p.Unidad,
                    Producido = delTipo.Sum(l => l.CantidadInicial),
                    Vendido = ventas.Where(v => v.TipoProducto == p.Tipo).Sum(v => v.Cantidad),
                    Disponible = delTipo.Sum(l => l.CantidadDisponible),
                    Danado = delTipo.Sum(l => l.CantidadDanada)
                };
            }).ToList();
        }

        /// <summary>Lotes con producto dañado, filtrados por fecha de vencimiento.</summary>
        public async Task<ReporteProductoDanadoResponse> ProductoDanadoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
        {
            Validaciones.ValidarRango(desde, hasta);
            var unidades = await UnidadesAsync(ct);
            var danados = (await _lotes.ObtenerPorRangoAsync(null, null, null, ct))
                .Where(l => l.CantidadDanada > 0
                         && (desde == null || l.FechaVencimiento >= desde)
                         && (hasta == null || l.FechaVencimiento <= hasta))
                .OrderBy(l => l.FechaVencimiento).ThenBy(l => l.Id)
                .ToList();

            return new ReporteProductoDanadoResponse
            {
                Lotes = danados.Select(l => new ReporteProductoDanadoItem
                {
                    LoteId = l.Id,
                    TipoProducto = l.Tipo,
                    FechaProduccion = l.FechaProduccion,
                    FechaVencimiento = l.FechaVencimiento,
                    CantidadDanada = l.CantidadDanada
                }).ToList(),
                PorTipo = danados.GroupBy(l => l.Tipo).OrderBy(g => g.Key).Select(g => new ReporteProductoDanadoTotal
                {
                    TipoProducto = g.Key,
                    Unidad = unidades.GetValueOrDefault(g.Key, string.Empty),
                    CantidadDanada = g.Sum(l => l.CantidadDanada),
                    Lotes = g.Count()
                }).ToList()
            };
        }

        /// <summary>Producción por vaca: promedio, mayor y menor productora y vacas bajo el promedio.</summary>
        public async Task<ReporteVacasResponse> VacasAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
        {
            Validaciones.ValidarRango(desde, hasta);
            var producciones = await _producciones.ObtenerPorRangoAsync(desde, hasta, ct);

            var vacas = producciones
                .GroupBy(p => p.VacaId)
                .Select(g => new ReporteVacaItem
                {
                    VacaId = g.Key,
                    Codigo = g.First().Vaca?.Codigo ?? string.Empty,
                    DiasRegistrados = g.Count(),
                    LitrosTotales = g.Sum(p => p.Litros),
                    PromedioDiario = Math.Round(g.Average(p => p.Litros), 2)
                })
                .OrderByDescending(v => v.PromedioDiario).ThenBy(v => v.VacaId)
                .ToList();

            var promedio = vacas.Count > 0 ? Math.Round(vacas.Average(v => v.PromedioDiario), 2) : 0;
            return new ReporteVacasResponse
            {
                Desde = desde,
                Hasta = hasta,
                ProduccionPromedio = promedio,
                MayorProductora = vacas.FirstOrDefault(),
                MenorProductora = vacas.LastOrDefault(),
                DebajoDelPromedio = vacas.Where(v => v.PromedioDiario < promedio).OrderBy(v => v.PromedioDiario).ToList(),
                Vacas = vacas
            };
        }

        private async Task<Dictionary<TipoProducto, string>> UnidadesAsync(CancellationToken ct) =>
            (await _granjas.ObtenerParametrosAsync(ct)).ToDictionary(p => p.Tipo, p => p.Unidad);
    }
}
