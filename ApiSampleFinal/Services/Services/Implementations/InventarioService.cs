using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Services.Dtos;
using Services.Interfaces;

namespace Services.Implementations
{
    public class InventarioService : IInventarioService
    {
        private readonly ILoteRepository _lotes;
        private readonly IGranjaRepository _granjas;
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;
        private readonly IMapper _mapper;

        public InventarioService(ILoteRepository lotes, IGranjaRepository granjas, IUnitOfWork uow, IClock clock, IMapper mapper)
        {
            _lotes = lotes;
            _granjas = granjas;
            _uow = uow;
            _clock = clock;
            _mapper = mapper;
        }

        /// <summary>Inventario I a una fecha: resumen por producto y lotes existentes.</summary>
        public async Task<InventarioResponse> ConsultarAsync(TipoProducto? tipo, DateOnly? fecha, bool incluirAgotados, CancellationToken ct = default)
        {
            var dia = fecha ?? _clock.Today;
            var lotes = await _lotes.ObtenerPorRangoAsync(null, dia, tipo, ct);
            var parametros = await _granjas.ObtenerParametrosAsync(ct);

            var resumen = parametros
                .Where(p => tipo == null || p.Tipo == tipo)
                .Select(p =>
                {
                    var delTipo = lotes.Where(l => l.Tipo == p.Tipo).ToList();
                    var vendibles = delTipo.Where(l => l.EsVendible(dia)).ToList();
                    return new InventarioProductoResponse
                    {
                        TipoProducto = p.Tipo,
                        Unidad = p.Unidad,
                        Vendible = vendibles.Sum(l => l.CantidadDisponible),
                        LotesVendibles = vendibles.Count,
                        VencidoSinProcesar = delTipo.Where(l => l.EstaVencido(dia)).Sum(l => l.CantidadDisponible)
                    };
                })
                .ToList();

            return new InventarioResponse
            {
                Fecha = dia,
                Productos = resumen,
                Lotes = lotes.Where(l => incluirAgotados || l.CantidadDisponible > 0)
                             .Select(l => ALoteResponse(l, dia))
                             .ToList()
            };
        }

        public async Task<LoteResponse> ObtenerLoteAsync(int id, DateOnly? fecha, CancellationToken ct = default)
        {
            var lote = await _lotes.ObtenerPorIdAsync(id, ct) ?? throw new EntidadNoEncontradaException("Lote", id);
            return ALoteResponse(lote, fecha ?? _clock.Today);
        }

        /// <summary>RN-08: la cantidad disponible de los lotes vencidos pasa a dañada.</summary>
        public async Task<ProcesarVencimientosResponse> ProcesarVencimientosAsync(DateOnly? fecha, CancellationToken ct = default)
        {
            var dia = fecha ?? _clock.Today;
            var vencidos = await _lotes.ObtenerVencidosConExistenciasAsync(dia, ct);
            foreach (var lote in vencidos)
                lote.ProcesarVencimiento(dia);

            await _uow.SaveChangesAsync(ct);
            return new ProcesarVencimientosResponse
            {
                Fecha = dia,
                LotesAfectados = vencidos.Count,
                Lotes = vencidos.Select(l => ALoteResponse(l, dia)).ToList()
            };
        }

        /// <summary>Extensión opcional: lotes vendibles que vencen en los próximos días.</summary>
        public async Task<IReadOnlyList<LoteResponse>> ProximosAVencerAsync(int dias, DateOnly? fecha, CancellationToken ct = default)
        {
            if (dias < 0) throw new DomainException("Los días deben ser cero o más.");
            var dia = fecha ?? _clock.Today;
            var lotes = await _lotes.ObtenerPorRangoAsync(null, dia, null, ct);
            return lotes.Where(l => l.VenceEnLosProximos(dia, dias))
                        .OrderBy(l => l.FechaVencimiento)
                        .Select(l => ALoteResponse(l, dia))
                        .ToList();
        }

        private LoteResponse ALoteResponse(LoteProducto lote, DateOnly fecha)
        {
            var response = _mapper.Map<LoteResponse>(lote);
            response.Estado = lote.ObtenerEstado(fecha);
            return response;
        }
    }
}
