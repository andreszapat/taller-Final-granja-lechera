using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
namespace Granja.Domain.Entities;
public sealed class Venta
{
    private Venta() { }
    public Venta(DateOnly fecha, TipoProducto tipo, decimal cantidad, decimal precioUnitario)
    {
        if (cantidad <= 0 || precioUnitario < 0) throw new ValidacionNegocioException("Cantidad inválida o precio unitario negativo.");
        Fecha = fecha; Tipo = tipo; Cantidad = cantidad; PrecioUnitario = precioUnitario;
        Total = cantidad * precioUnitario;
    }
    public int Id { get; private set; }
    public DateOnly Fecha { get; private set; }
    public TipoProducto Tipo { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Total { get; private set; }
    public List<VentaDetalle> Detalles { get; private set; } = new();
    public void AgregarDetalle(int loteId, DateOnly fechaProduccionLote, decimal cantidad)
    {
        Detalles.Add(new VentaDetalle(loteId, fechaProduccionLote, cantidad));
    }
}
