using Domain.Enums;
using Services.Dtos;

namespace Services.Interfaces
{
    public interface IGranjaService
    {
        Task<GranjaResponse> ObtenerAsync(CancellationToken ct = default);
        Task<GranjaResponse> ActualizarAsync(ActualizarGranjaRequest request, CancellationToken ct = default);
    }

    public interface IVacaService
    {
        Task<IReadOnlyList<VacaResponse>> ListarAsync(CancellationToken ct = default);
        Task<VacaResponse> ObtenerAsync(int id, CancellationToken ct = default);
        Task<VacaResponse> CrearAsync(CrearVacaRequest request, CancellationToken ct = default);
    }

    public interface IProduccionService
    {
        Task<ProduccionDiaResponse> RegistrarDiaAsync(RegistrarProduccionRequest request, CancellationToken ct = default);
        Task<ProduccionDiaResponse> RegistrarVacaAsync(RegistrarProduccionVacaRequest request, CancellationToken ct = default);
        Task<ProduccionDiaResponse> ObtenerDiaAsync(DateOnly fecha, CancellationToken ct = default);
        Task<IReadOnlyList<ProduccionDiaResponse>> ListarAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
        Task<LoteResponse> TransformarAsync(TransformacionRequest request, CancellationToken ct = default);
    }

    public interface IInventarioService
    {
        Task<InventarioResponse> ConsultarAsync(TipoProducto? tipo, DateOnly? fecha, bool incluirAgotados, CancellationToken ct = default);
        Task<LoteResponse> ObtenerLoteAsync(int id, DateOnly? fecha, CancellationToken ct = default);
        Task<ProcesarVencimientosResponse> ProcesarVencimientosAsync(DateOnly? fecha, CancellationToken ct = default);
        Task<IReadOnlyList<LoteResponse>> ProximosAVencerAsync(int dias, DateOnly? fecha, CancellationToken ct = default);
    }

    public interface IVentaService
    {
        Task<VentaResponse> RegistrarVentaAsync(VentaRequest request, CancellationToken ct = default);
        Task<VentaResponse> ObtenerAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<VentaResponse>> ListarAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default);
    }

    public interface IReporteService
    {
        Task<ReporteVentasResponse> VentasAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default);
        Task<ReporteLecheDiariaResponse> LecheDiariaAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
        Task<IReadOnlyList<ReporteProductoDiarioItem>> ProductoDiarioAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
        Task<IReadOnlyList<ReporteProductoTotalItem>> ProductoTotalAsync(CancellationToken ct = default);
        Task<ReporteProductoDanadoResponse> ProductoDanadoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
        Task<ReporteVacasResponse> VacasAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    }
}
