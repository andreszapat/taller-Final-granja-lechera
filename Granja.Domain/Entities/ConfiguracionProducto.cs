using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
namespace Granja.Domain.Entities;
public sealed class ConfiguracionProducto
{
    private ConfiguracionProducto() { }
    public ConfiguracionProducto(TipoProducto tipo, decimal rendimientoLitrosPorUnidad, int vidaUtilDias, decimal precioUnitario)
    { Tipo = tipo; Configurar(rendimientoLitrosPorUnidad, vidaUtilDias, precioUnitario); }
    public TipoProducto Tipo { get; private set; }
    public decimal RendimientoLitrosPorUnidad { get; private set; }
    public int VidaUtilDias { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public void Configurar(decimal rendimientoLitrosPorUnidad, int vidaUtilDias, decimal precioUnitario)
    {
        if (rendimientoLitrosPorUnidad <= 0 || vidaUtilDias <= 0 || precioUnitario < 0)
            throw new ValidacionNegocioException("El rendimiento y los días de vida útil deben ser positivos; el precio no puede ser negativo.");
        RendimientoLitrosPorUnidad = rendimientoLitrosPorUnidad;
        VidaUtilDias = vidaUtilDias;
        PrecioUnitario = precioUnitario;
    }
}
