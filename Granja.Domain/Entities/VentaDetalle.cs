namespace Granja.Domain.Entities;
public sealed class VentaDetalle
{
    private VentaDetalle() { }
    internal VentaDetalle(int loteId, DateOnly fechaProduccion, decimal cantidad)
    { LoteId = loteId; FechaProduccion = fechaProduccion; Cantidad = cantidad; }
    public int Id { get; private set; }
    public int VentaId { get; private set; }
    public Venta Venta { get; private set; } = null!;
    public int LoteId { get; private set; }
    public LoteProducto Lote { get; private set; } = null!;
    public DateOnly FechaProduccion { get; private set; }
    public decimal Cantidad { get; private set; }
}
