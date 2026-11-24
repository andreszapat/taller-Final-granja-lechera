using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Services.Dtos;
using Services.Interfaces;

namespace Services.Implementations
{
    public class GranjaService : IGranjaService
    {
        private readonly IGranjaRepository _granjas;
        private readonly IVacaRepository _vacas;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GranjaService(IGranjaRepository granjas, IVacaRepository vacas, IUnitOfWork uow, IMapper mapper)
        {
            _granjas = granjas;
            _vacas = vacas;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<GranjaResponse> ObtenerAsync(CancellationToken ct = default)
        {
            var granja = await _granjas.ObtenerAsync(ct);
            var parametros = await _granjas.ObtenerParametrosAsync(ct);
            var response = _mapper.Map<GranjaResponse>(granja);
            response.Productos = _mapper.Map<List<ProductoParametroDto>>(parametros);
            return response;
        }

        public async Task<GranjaResponse> ActualizarAsync(ActualizarGranjaRequest request, CancellationToken ct = default)
        {
            var granja = await _granjas.ObtenerAsync(ct);
            granja.Actualizar(request.NumeroVacas, request.LitrosPorVaca);
            if (!string.IsNullOrWhiteSpace(request.Nombre))
                granja.Renombrar(request.Nombre);

            if (request.Productos is { Count: > 0 })
            {
                if (request.Productos.GroupBy(p => p.TipoProducto).Any(g => g.Count() > 1))
                    throw new DomainException("Cada producto solo puede aparecer una vez en la lista.");

                foreach (var dto in request.Productos)
                {
                    var parametro = await _granjas.ObtenerParametroAsync(dto.TipoProducto!.Value, ct);
                    parametro.Actualizar(dto.Unidad, dto.LitrosPorUnidad, dto.VidaUtilDias, dto.Precio);
                }
            }

            await SincronizarVacasAsync(request.NumeroVacas, ct);
            await _uow.SaveChangesAsync(ct);
            return await ObtenerAsync(ct);
        }

        /// <summary>Deja exactamente n vacas activas: reactiva o crea las que falten y desactiva las sobrantes.</summary>
        private async Task SincronizarVacasAsync(int n, CancellationToken ct)
        {
            var todas = await _vacas.ListarAsync(ct);
            var activas = todas.Where(v => v.Activa).OrderBy(v => v.Id).ToList();

            if (activas.Count > n)
            {
                foreach (var vaca in activas.Skip(n))
                    vaca.Desactivar();
                return;
            }

            var faltan = n - activas.Count;
            foreach (var vaca in todas.Where(v => !v.Activa).OrderBy(v => v.Id).Take(faltan).ToList())
            {
                vaca.Activar();
                faltan--;
            }

            var codigos = todas.Select(v => v.Codigo).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var consecutivo = todas.Count;
            while (faltan > 0)
            {
                string codigo;
                do { codigo = $"V-{++consecutivo:000}"; } while (!codigos.Add(codigo));
                await _vacas.AgregarAsync(new Vaca(codigo, "Sin especificar"), ct);
                faltan--;
            }
        }
    }

    public class VacaService : IVacaService
    {
        private readonly IVacaRepository _vacas;
        private readonly IGranjaRepository _granjas;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public VacaService(IVacaRepository vacas, IGranjaRepository granjas, IUnitOfWork uow, IMapper mapper)
        {
            _vacas = vacas;
            _granjas = granjas;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<VacaResponse>> ListarAsync(CancellationToken ct = default) =>
            _mapper.Map<List<VacaResponse>>(await _vacas.ListarAsync(ct));

        public async Task<VacaResponse> ObtenerAsync(int id, CancellationToken ct = default) =>
            _mapper.Map<VacaResponse>(await _vacas.ObtenerPorIdAsync(id, ct)
                ?? throw new EntidadNoEncontradaException("Vaca", id));

        /// <summary>Agrega una vaca activa; n (número de vacas) se actualiza para coincidir.</summary>
        public async Task<VacaResponse> CrearAsync(CrearVacaRequest request, CancellationToken ct = default)
        {
            var todas = await _vacas.ListarAsync(ct);
            if (todas.Any(v => v.Codigo.Equals(request.Codigo.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new DatoDuplicadoException($"Ya existe una vaca con el código '{request.Codigo}'.");

            var vaca = new Vaca(request.Codigo, request.Raza);
            await _vacas.AgregarAsync(vaca, ct);

            var granja = await _granjas.ObtenerAsync(ct);
            granja.Actualizar(todas.Count(v => v.Activa) + 1, granja.LitrosPorVaca);

            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<VacaResponse>(vaca);
        }
    }
}
