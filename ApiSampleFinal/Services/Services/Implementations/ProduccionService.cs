using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Services.Dtos;
using Services.Interfaces;

namespace Services.Implementations
{
    public class ProduccionService : IProduccionService
    {
        private readonly IGranjaRepository _granjas;
        private readonly IVacaRepository _vacas;
        private readonly IProduccionLecheRepository _producciones;
        private readonly ILoteRepository _lotes;
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;
        private readonly IMapper _mapper;

        public ProduccionService(IGranjaRepository granjas, IVacaRepository vacas, IProduccionLecheRepository producciones,
            ILoteRepository lotes, IUnitOfWork uow, IClock clock, IMapper mapper)
        {
            _granjas = granjas;
            _vacas = vacas;
            _producciones = producciones;
            _lotes = lotes;
            _uow = uow;
            _clock = clock;
            _mapper = mapper;
        }

        /// <summary>RN-01: registra la producción del día de todas las vacas activas (n × l por defecto).</summary>
        public async Task<ProduccionDiaResponse> RegistrarDiaAsync(RegistrarProduccionRequest request, CancellationToken ct = default)
        {
            var fecha = request.Fecha ?? _clock.Today;
            if (await _producciones.ExisteAsync(fecha, null, ct))
                throw new ProduccionDuplicadaException(fecha);

            var granja = await _granjas.ObtenerAsync(ct);
            var vacas = await _vacas.ListarActivasAsync(ct);
            if (vacas.Count == 0)
                throw new DomainException("La granja no tiene vacas activas.");

            var ajustes = request.Ajustes ?? new List<ProduccionVacaRequest>();
            if (ajustes.GroupBy(a => a.VacaId).Any(g => g.Count() > 1))
                throw new DomainException("Una vaca aparece más de una vez en los ajustes.");
            var noActivas = ajustes.Select(a => a.VacaId).Except(vacas.Select(v => v.Id)).ToList();
            if (noActivas.Count > 0)
                throw new EntidadNoEncontradaException("Vaca activa", string.Join(", ", noActivas));

            var litrosPorVaca = ajustes.ToDictionary(a => a.VacaId, a => a.Litros);
            foreach (var vaca in vacas)
            {
                var litros = litrosPorVaca.TryGetValue(vaca.Id, out var l) ? l : granja.LitrosPorVaca;
                await _producciones.AgregarAsync(new ProduccionLeche(vaca.Id, fecha, litros), ct);
            }

            await _uow.SaveChangesAsync(ct);
            return await ObtenerDiaAsync(fecha, ct);
        }

        /// <summary>RN-01: registra la producción de una sola vaca (una vez por fecha).</summary>
        public async Task<ProduccionDiaResponse> RegistrarVacaAsync(RegistrarProduccionVacaRequest request, CancellationToken ct = default)
        {
            var fecha = request.Fecha ?? _clock.Today;
            var vaca = await _vacas.ObtenerPorIdAsync(request.VacaId, ct)
                       ?? throw new EntidadNoEncontradaException("Vaca", request.VacaId);
            if (!vaca.Activa)
                throw new DomainException($"La vaca {vaca.Codigo} está inactiva.");
            if (await _producciones.ExisteAsync(fecha, vaca.Id, ct))
                throw new ProduccionDuplicadaException(fecha, vaca.Id);

            var litros = request.Litros ?? (await _granjas.ObtenerAsync(ct)).LitrosPorVaca;
            await _producciones.AgregarAsync(new ProduccionLeche(vaca.Id, fecha, litros), ct);
            await _uow.SaveChangesAsync(ct);
            return await ObtenerDiaAsync(fecha, ct);
        }

        public async Task<ProduccionDiaResponse> ObtenerDiaAsync(DateOnly fecha, CancellationToken ct = default)
        {
            var dia = (await ListarAsync(fecha, fecha, ct)).FirstOrDefault();
            return dia ?? throw new EntidadNoEncontradaException("Producción de leche", fecha.ToString("yyyy-MM-dd"));
        }

        public async Task<IReadOnlyList<ProduccionDiaResponse>> ListarAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
        {
            Validaciones.ValidarRango(desde, hasta);
            var producciones = await _producciones.ObtenerPorRangoAsync(desde, hasta, ct);
            var lotes = await _lotes.ObtenerPorRangoAsync(desde, hasta, null, ct);
            var transformados = lotes.GroupBy(l => l.FechaProduccion).ToDictionary(g => g.Key, g => g.Sum(l => l.LitrosUsados));

            return producciones
                .GroupBy(p => p.Fecha)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var producidos = g.Sum(p => p.Litros);
                    var usados = transformados.GetValueOrDefault(g.Key);
                    return new ProduccionDiaResponse
                    {
                        Fecha = g.Key,
                        VacasOrdenadas = g.Count(),
                        LitrosProducidos = producidos,
                        LitrosTransformados = usados,
                        LitrosPorTransformar = producidos - usados,
                        Detalle = _mapper.Map<List<ProduccionVacaResponse>>(g.ToList())
                    };
                })
                .ToList();
        }

        /// <summary>RN-02 a RN-04: transforma litros de una fecha en un lote de queso o yogurt.</summary>
        public async Task<LoteResponse> TransformarAsync(TransformacionRequest request, CancellationToken ct = default)
        {
            var fecha = request.Fecha ?? _clock.Today;
            var tipo = request.TipoProducto!.Value;
            if (request.Litros <= 0)
                throw new DomainException("Los litros a transformar deben ser mayores que cero.");

            var producidos = await _producciones.LitrosDelDiaAsync(fecha, ct);
            var transformados = await _lotes.LitrosTransformadosAsync(fecha, ct);
            var disponibles = producidos - transformados;
            if (request.Litros > disponibles)
                throw new LecheInsuficienteException(fecha, request.Litros, disponibles);

            var parametro = await _granjas.ObtenerParametroAsync(tipo, ct);
            var lote = new LoteProducto(parametro, fecha, request.Litros);
            await _lotes.AgregarAsync(lote, ct);
            await _uow.SaveChangesAsync(ct);

            var response = _mapper.Map<LoteResponse>(lote);
            response.Estado = lote.ObtenerEstado(fecha);
            return response;
        }
    }

    internal static class Validaciones
    {
        public static void ValidarRango(DateOnly? desde, DateOnly? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde > hasta)
                throw new DomainException("La fecha 'desde' no puede ser posterior a 'hasta'.");
        }
    }
}
