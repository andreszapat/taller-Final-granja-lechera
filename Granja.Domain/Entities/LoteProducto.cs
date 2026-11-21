using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
namespace Granja.Domain.Entities;
public sealed class LoteProducto
{
    private LoteProducto() { }
    internal LoteProducto(TipoProducto tipo, DateOnly fechaProduccion, int vidaUtilDias, decimal cantidad)
    {
        if (cantidad <= 0 || vidaUtilDias <= 0) throw new ValidacionNegocioException("El lote requiere una cantidad y una vida útil positivas.");
        Tipo = tipo; FechaProduccion = fechaProduccion; FechaVencimiento = fechaProduccion.AddDays(vidaUtilDias);
        CantidadInicial = cantidad; CantidadDisponible = cantidad;
        ConcurrencyToken = Guid.NewGuid();
    }
    public int Id { get; private set; }
    public int TransformacionId { get; private set; }
    public Transformacion Transformacion { get; private set; } = null!;
    public TipoProducto Tipo { get; private set; }
    public DateOnly FechaProduccion { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }
    public decimal CantidadInicial { get; private set; }
    public decimal CantidadDisponible { get; private set; }
    public decimal CantidadDanada { get; private set; }
    public DateOnly? FechaProcesoDanado { get; private set; }
    public Guid ConcurrencyToken { get; private set; }
    public bool EstaVencido(DateOnly fecha) => fecha > FechaVencimiento;
    public void Descontar(decimal cantidad)
    {
        if (cantidad <= 0 || cantidad > CantidadDisponible)
            throw new ValidacionNegocioException("Cantidad inválida para descontar del lote.");
        CantidadDisponible -= cantidad;
        ConcurrencyToken = Guid.NewGuid();
    }
    public decimal MarcarComoDanado(DateOnly fecha)
    {
        if (!EstaVencido(fecha)) throw new ValidacionNegocioException("No puede marcarse como dañado un lote no vencido.");
        var perdida = CantidadDisponible;
        if (perdida <= 0) return 0;
        CantidadDanada += perdida;
        CantidadDisponible = 0;
        FechaProcesoDanado = fecha;
        ConcurrencyToken = Guid.NewGuid();
        return perdida;
    }
}
