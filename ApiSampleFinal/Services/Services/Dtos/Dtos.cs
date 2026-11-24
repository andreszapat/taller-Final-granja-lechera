using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Services.Dtos
{
    // ---------- Granja y parámetros ----------

    public class ProductoParametroDto
    {
        [Required, EnumDataType(typeof(TipoProducto))]
        public TipoProducto? TipoProducto { get; set; }
        [Required, StringLength(10)]
        public string Unidad { get; set; } = string.Empty;
        /// <summary>Rendimiento: litros de leche por unidad de producto.</summary>
        [Range(0.001, 100000)]
        public decimal LitrosPorUnidad { get; set; }
        [Range(1, 3650)]
        public int VidaUtilDias { get; set; }
        [Range(0, 1000000000)]
        public decimal Precio { get; set; }
    }

    public class GranjaResponse
    {
        public string Nombre { get; set; } = string.Empty;
        /// <summary>n: número de vacas.</summary>
        public int NumeroVacas { get; set; }
        /// <summary>l: litros por vaca por día.</summary>
        public decimal LitrosPorVaca { get; set; }
        /// <summary>n × l.</summary>
        public decimal ProduccionDiariaEsperada { get; set; }
        public List<ProductoParametroDto> Productos { get; set; } = new();
    }

    public class ActualizarGranjaRequest
    {
        [StringLength(120)]
        public string? Nombre { get; set; }
        [Range(1, 100000)]
        public int NumeroVacas { get; set; }
        [Range(0.001, 1000)]
        public decimal LitrosPorVaca { get; set; }
        /// <summary>Parámetros por producto a actualizar (opcional).</summary>
        public List<ProductoParametroDto>? Productos { get; set; }
    }

    // ---------- Vacas ----------

    public class VacaResponse
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Raza { get; set; } = string.Empty;
        public bool Activa { get; set; }
    }

    public class CrearVacaRequest
    {
        [Required, StringLength(20)]
        public string Codigo { get; set; } = string.Empty;
        [StringLength(60)]
        public string Raza { get; set; } = string.Empty;
    }

    // ---------- Producción de leche ----------

    public class ProduccionVacaRequest
    {
        [Range(1, int.MaxValue)]
        public int VacaId { get; set; }
        [Range(0.001, 1000)]
        public decimal Litros { get; set; }
    }

    public class RegistrarProduccionRequest
    {
        /// <summary>Fecha de la producción; si se omite se usa la fecha actual (IClock).</summary>
        public DateOnly? Fecha { get; set; }
        /// <summary>
        /// Litros de vacas que produjeron distinto de l (opcional). Las vacas activas que no
        /// aparezcan aquí producen el valor configurado l.
        /// </summary>
        public List<ProduccionVacaRequest>? Ajustes { get; set; }
    }

    public class RegistrarProduccionVacaRequest
    {
        [Range(1, int.MaxValue)]
        public int VacaId { get; set; }
        /// <summary>Litros producidos; si se omite se usa l.</summary>
        [Range(0.001, 1000)]
        public decimal? Litros { get; set; }
        public DateOnly? Fecha { get; set; }
    }

    public class ProduccionVacaResponse
    {
        public int VacaId { get; set; }
        public string CodigoVaca { get; set; } = string.Empty;
        public decimal Litros { get; set; }
    }

    public class ProduccionDiaResponse
    {
        public DateOnly Fecha { get; set; }
        public int VacasOrdenadas { get; set; }
        public decimal LitrosProducidos { get; set; }
        public decimal LitrosTransformados { get; set; }
        /// <summary>Litros del día que aún se pueden transformar.</summary>
        public decimal LitrosPorTransformar { get; set; }
        public List<ProduccionVacaResponse> Detalle { get; set; } = new();
    }

    public class TransformacionRequest
    {
        /// <summary>Fecha de la leche a transformar; si se omite se usa la fecha actual.</summary>
        public DateOnly? Fecha { get; set; }
        [Required, EnumDataType(typeof(TipoProducto))]
        public TipoProducto? TipoProducto { get; set; }
        [Range(0.001, 1000000)]
        public decimal Litros { get; set; }
    }

    // ---------- Inventario ----------

    public class LoteResponse
    {
        public int Id { get; set; }
        public TipoProducto TipoProducto { get; set; }
        public DateOnly FechaProduccion { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public decimal LitrosUsados { get; set; }
        public decimal CantidadInicial { get; set; }
        public decimal CantidadDisponible { get; set; }
        public decimal CantidadVendida { get; set; }
        public decimal CantidadDanada { get; set; }
        public EstadoLote Estado { get; set; }
    }

    public class InventarioProductoResponse
    {
        public TipoProducto TipoProducto { get; set; }
        public string Unidad { get; set; } = string.Empty;
        /// <summary>Existencias que se pueden vender en la fecha (no vencidas).</summary>
        public decimal Vendible { get; set; }
        /// <summary>Existencias vencidas que aún no se han procesado como dañadas.</summary>
        public decimal VencidoSinProcesar { get; set; }
        public int LotesVendibles { get; set; }
    }

    public class InventarioResponse
    {
        public DateOnly Fecha { get; set; }
        public List<InventarioProductoResponse> Productos { get; set; } = new();
        public List<LoteResponse> Lotes { get; set; } = new();
    }

    public class ProcesarVencimientosRequest
    {
        public DateOnly? Fecha { get; set; }
    }

    public class ProcesarVencimientosResponse
    {
        public DateOnly Fecha { get; set; }
        public int LotesAfectados { get; set; }
        public List<LoteResponse> Lotes { get; set; } = new();
    }

    // ---------- Ventas ----------

    public class VentaRequest
    {
        [Required, EnumDataType(typeof(TipoProducto))]
        public TipoProducto? TipoProducto { get; set; }
        [Range(0.001, 1000000)]
        public decimal Cantidad { get; set; }
        public DateOnly? Fecha { get; set; }
    }

    public class VentaDetalleResponse
    {
        public int LoteId { get; set; }
        public DateOnly FechaProduccion { get; set; }
        public decimal Cantidad { get; set; }
    }

    public class VentaResponse
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public TipoProducto TipoProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public List<VentaDetalleResponse> Detalle { get; set; } = new();
    }
}
