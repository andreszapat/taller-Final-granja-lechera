using Granja.Domain.Exceptions;
namespace Granja.Domain.Entities;
public sealed class ProduccionLeche
{
    private ProduccionLeche() { }
    public ProduccionLeche(int vacaId, DateOnly fecha, decimal litros)
    {
        if (vacaId <= 0 || litros < 0) throw new ValidacionNegocioException("La vaca debe existir y los litros no pueden ser negativos.");
        VacaId = vacaId; Fecha = fecha; Litros = litros;
    }
    public int Id { get; private set; }
    public int VacaId { get; private set; }
    public Vaca Vaca { get; private set; } = null!;
    public DateOnly Fecha { get; private set; }
    public decimal Litros { get; private set; }
}
