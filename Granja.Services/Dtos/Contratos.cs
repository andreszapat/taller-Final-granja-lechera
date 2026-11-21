using Granja.Domain.Enums;
namespace Granja.Services.Dtos;
public sealed record ParametrosProductoRequest(decimal RendimientoLitrosPorUnidad, int VidaUtilDias, decimal PrecioUnitario);
public sealed record GranjaActualizarRequest(int NumeroVacas, decimal LitrosPorVaca, ParametrosProductoRequest Queso, ParametrosProductoRequest Yogurt);
public sealed record GranjaConfigResponse(int NumeroVacas, decimal LitrosPorVaca, decimal LecheEsperadaPorDia,
    ParametrosProductoRequest Queso, ParametrosProductoRequest Yogurt);
public sealed record RenombrarVacaRequest(string Nombre);
public sealed class VacaResponse
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activa { get; set; }
}
public sealed record ProduccionVacaRequest(int VacaId, decimal Litros);
public sealed record ProduccionRequest(DateOnly? Fecha, List<ProduccionVacaRequest>? Vacas);
public sealed record ProduccionResponse(DateOnly Fecha, decimal TotalLitros, List<ProduccionVacaRequest> Vacas);
public sealed record TransformacionRequest(TipoProducto TipoProducto, decimal Litros, DateOnly? Fecha);
public sealed record TransformacionResponse(int Id, int LoteId, DateOnly Fecha, TipoProducto TipoProducto,
    decimal LitrosUsados, decimal CantidadObtenida, DateOnly FechaVencimiento);
public sealed record VentaRequest(TipoProducto TipoProducto, decimal Cantidad, DateOnly? Fecha);
public sealed record VentaDetalleResponse(int LoteId, DateOnly FechaProduccion, decimal Cantidad);
public sealed record VentaResponse(int Id, DateOnly Fecha, TipoProducto TipoProducto, decimal Cantidad,
    decimal PrecioUnitario, decimal Total, List<VentaDetalleResponse> Detalle);
public sealed record VencimientoRequest(DateOnly? Fecha);
public sealed record VencimientosResponse(DateOnly Fecha, int LotesProcesados, decimal UnidadesDanadas,
    List<InventarioLoteResponse> Lotes);
public sealed class InventarioLoteResponse
{
    public int Id { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public DateOnly FechaProduccion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public decimal CantidadInicial { get; set; }
    public decimal CantidadDisponible { get; set; }
    public decimal CantidadVendible { get; set; }
    public decimal CantidadDanada { get; set; }
    public DateOnly? FechaProcesoDanado { get; set; }
    public string Estado { get; set; } = "";
}
public sealed record VentasReporteItem(int Id, DateOnly Fecha, TipoProducto Producto,
    decimal Cantidad, decimal PrecioUnitario, decimal Total);
public sealed record VentasReporteResponse(List<VentasReporteItem> Ventas, decimal TotalGeneral);
public sealed record LecheDiariaItem(DateOnly Fecha, decimal LitrosProducidos,
    decimal LitrosTransformados, decimal LitrosNoAprovechados);
public sealed record ProductoDiarioItem(DateOnly Fecha, TipoProducto Producto, decimal CantidadProducida, decimal LitrosConsumidos);
public sealed record ProductoTotalItem(TipoProducto Producto, decimal Producido, decimal Vendido, decimal Disponible,
    decimal Danado, decimal VencidoPendienteDeProcesar);
public sealed record ProductoDanadoItem(int LoteId, TipoProducto Producto, DateOnly FechaProduccion,
    DateOnly FechaVencimiento, DateOnly? FechaProcesoDanado, decimal CantidadDanada);
public sealed record ProduccionVacaItem(int VacaId, string Codigo, string Nombre, decimal TotalLitros,
    int DiasRegistrados, decimal PromedioLitrosPorDia);
public sealed record ReporteVacasResponse(decimal PromedioGeneralPorVaca, List<ProduccionVacaItem> Vacas,
    List<ProduccionVacaItem> MayorProduccion, List<ProduccionVacaItem> MenorProduccion,
    List<ProduccionVacaItem> PorDebajoDelPromedio);
