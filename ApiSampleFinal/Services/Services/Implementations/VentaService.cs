using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Services.Dtos;
using Services.Interfaces;

namespace Services.Implementations
{
    public class VentaService : IVentaService
    {
        private readonly ILoteRepository _lotes;
        private readonly IVentaRepository _ventas;
        private readonly IGranjaRepository _granjas;
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;
        private readonly IMapper _mapper;

        public VentaService(ILoteRepository lotes, IVentaRepository ventas, IGranjaRepository granjas,
            IUnitOfWork uow, IClock clock, IMapper mapper)
        {
            _lotes = lotes;
            _ventas = ventas;
            _granjas = granjas;
            _uow = uow;
            _clock = clock;
            _mapper = mapper;
        }

        /// <summary>
        /// RN-05 a RN-09. Despacha FIFO: primero el lote más antiguo no vencido; si no alcanza
        /// sigue con el siguiente. Si no hay existencias suficientes se rechaza sin tocar nada.
        /// </summary>
        public async Task<VentaResponse> RegistrarVentaAsync(VentaRequest request, CancellationToken ct = default)
        {
            var fecha = request.Fecha ?? _clock.Today;
            var tipo = request.TipoProducto!.Value;
            if (request.Cantidad <= 0)
                throw new DomainException("La cantidad a vender debe ser mayor que cero.");

            var lotes = await _lotes.ObtenerVendiblesAsync(tipo, fecha, ct); // FechaProduccion ASC
            var existencias = lotes.Sum(l => l.CantidadDisponible);
            if (existencias < request.Cantidad)
                throw new InventarioInsuficienteException(tipo, request.Cantidad, existencias);

            var precio = (await _granjas.ObtenerParametroAsync(tipo, ct)).Precio;
            var venta = new Venta(fecha, tipo, precio);
            var restante = request.Cantidad;

            foreach (var lote in lotes)
            {
                if (restante == 0) break;
                var tomar = Math.Min(lote.CantidadDisponible, restante);
                venta.AgregarDetalle(lote, tomar); // descuenta del lote y guarda la trazabilidad
                restante -= tomar;
            }

            await _ventas.AgregarAsync(venta, ct);
            await _uow.SaveChangesAsync(ct); // una sola transacción
            return _mapper.Map<VentaResponse>(venta);
        }

        public async Task<VentaResponse> ObtenerAsync(int id, CancellationToken ct = default) =>
            _mapper.Map<VentaResponse>(await _ventas.ObtenerPorIdAsync(id, ct)
                ?? throw new EntidadNoEncontradaException("Venta", id));

        public async Task<IReadOnlyList<VentaResponse>> ListarAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default)
        {
            Validaciones.ValidarRango(desde, hasta);
            return _mapper.Map<List<VentaResponse>>(await _ventas.ObtenerPorRangoAsync(desde, hasta, tipo, ct));
        }
    }
}
