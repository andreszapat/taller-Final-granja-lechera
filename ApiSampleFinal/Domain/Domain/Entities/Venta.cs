using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities
{
    /// <summary>Venta de un producto. Guarda precio, cantidad, total y los lotes de origen (RN-09).</summary>
    public class Venta
    {
        public int Id { get; private set; }
        public DateOnly Fecha { get; private set; }
        public TipoProducto TipoProducto { get; private set; }
        public decimal Cantidad { get; private set; }
        public decimal PrecioUnitario { get; private set; }
        public decimal Total { get; private set; }

        private readonly List<VentaDetalle> _detalles = new();
        public IReadOnlyCollection<VentaDetalle> Detalles => _detalles;

        private Venta() { }

        public Venta(DateOnly fecha, TipoProducto tipoProducto, decimal precioUnitario)
        {
            if (precioUnitario < 0) throw new DomainException("El precio unitario no puede ser negativo.");
            Fecha = fecha;
            TipoProducto = tipoProducto;
            PrecioUnitario = precioUnitario;
        }

        /// <summary>Despacha <paramref name="cantidad"/> del lote y deja la trazabilidad en el detalle.</summary>
        public void AgregarDetalle(LoteProducto lote, decimal cantidad)
        {
            if (lote.Tipo != TipoProducto)
                throw new DomainException($"El lote {lote.Id} es de {lote.Tipo} y la venta es de {TipoProducto}.");
            if (!lote.EsVendible(Fecha))
                throw new DomainException($"El lote {lote.Id} no se puede vender el {Fecha:yyyy-MM-dd}.");

            lote.Descontar(cantidad);
            _detalles.Add(new VentaDetalle(lote, cantidad));
            Cantidad += cantidad;
            Total = Cantidad * PrecioUnitario;
        }
    }

    /// <summary>Cantidad que una venta tomó de un lote (trazabilidad FIFO).</summary>
    public class VentaDetalle
    {
        public int Id { get; private set; }
        public int VentaId { get; private set; }
        public int LoteId { get; private set; }
        public LoteProducto? Lote { get; private set; }
        public decimal Cantidad { get; private set; }

        private VentaDetalle() { }

        internal VentaDetalle(LoteProducto lote, decimal cantidad)
        {
            Lote = lote;
            LoteId = lote.Id;
            Cantidad = cantidad;
        }
    }
}
