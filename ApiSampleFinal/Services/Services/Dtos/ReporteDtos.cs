using Domain.Enums;

namespace Services.Dtos
{
    public class ReporteVentaItem
    {
        public int VentaId { get; set; }
        public DateOnly Fecha { get; set; }
        public TipoProducto TipoProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
    }

    public class ReporteVentasResponse
    {
        public DateOnly? Desde { get; set; }
        public DateOnly? Hasta { get; set; }
        public TipoProducto? Tipo { get; set; }
        public List<ReporteVentaItem> Ventas { get; set; } = new();
        public decimal CantidadTotal { get; set; }
        public decimal TotalGeneral { get; set; }
    }

    public class ReporteLecheDiariaItem
    {
        public DateOnly Fecha { get; set; }
        public int VacasOrdenadas { get; set; }
        public decimal LitrosProducidos { get; set; }
        public decimal LitrosTransformados { get; set; }
        public decimal LitrosNoAprovechados { get; set; }
    }

    public class ReporteLecheDiariaResponse
    {
        public List<ReporteLecheDiariaItem> Dias { get; set; } = new();
        public decimal TotalProducido { get; set; }
        public decimal TotalTransformado { get; set; }
        public decimal TotalNoAprovechado { get; set; }
    }

    public class ReporteProductoDiarioItem
    {
        public DateOnly Fecha { get; set; }
        public TipoProducto TipoProducto { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public int Lotes { get; set; }
        public decimal LitrosUsados { get; set; }
        public decimal Cantidad { get; set; }
    }

    public class ReporteProductoTotalItem
    {
        public TipoProducto TipoProducto { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public decimal Producido { get; set; }
        public decimal Vendido { get; set; }
        public decimal Disponible { get; set; }
        public decimal Danado { get; set; }
    }

    public class ReporteProductoDanadoItem
    {
        public int LoteId { get; set; }
        public TipoProducto TipoProducto { get; set; }
        public DateOnly FechaProduccion { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public decimal CantidadDanada { get; set; }
    }

    public class ReporteProductoDanadoTotal
    {
        public TipoProducto TipoProducto { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public decimal CantidadDanada { get; set; }
        public int Lotes { get; set; }
    }

    public class ReporteProductoDanadoResponse
    {
        public List<ReporteProductoDanadoItem> Lotes { get; set; } = new();
        public List<ReporteProductoDanadoTotal> PorTipo { get; set; } = new();
    }

    public class ReporteVacaItem
    {
        public int VacaId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int DiasRegistrados { get; set; }
        public decimal LitrosTotales { get; set; }
        /// <summary>Litros promedio por día registrado.</summary>
        public decimal PromedioDiario { get; set; }
    }

    public class ReporteVacasResponse
    {
        public DateOnly? Desde { get; set; }
        public DateOnly? Hasta { get; set; }
        /// <summary>Promedio de los promedios diarios de todas las vacas.</summary>
        public decimal ProduccionPromedio { get; set; }
        public ReporteVacaItem? MayorProductora { get; set; }
        public ReporteVacaItem? MenorProductora { get; set; }
        public List<ReporteVacaItem> DebajoDelPromedio { get; set; } = new();
        public List<ReporteVacaItem> Vacas { get; set; } = new();
    }
}
