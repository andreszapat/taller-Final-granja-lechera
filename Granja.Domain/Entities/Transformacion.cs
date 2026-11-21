using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
namespace Granja.Domain.Entities;
public sealed class Transformacion
{
    private Transformacion() { }
    public Transformacion(TipoProducto tipo, DateOnly fecha, decimal litrosUsados, decimal rendimiento, int vidaUtilDias)
    {
        if (litrosUsados <= 0 || rendimiento <= 0 || vidaUtilDias <= 0)
            throw new ValidacionNegocioException("La transformación requiere litros, rendimiento y vida útil positivos.");
        Tipo = tipo; Fecha = fecha; LitrosUsados = litrosUsados;
        CantidadObtenida = litrosUsados / rendimiento;
        Lote = new LoteProducto(tipo, fecha, vidaUtilDias, CantidadObtenida);
    }
    public int Id { get; private set; }
    public TipoProducto Tipo { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal LitrosUsados { get; private set; }
    public decimal CantidadObtenida { get; private set; }
    public LoteProducto Lote { get; private set; } = null!;
}
