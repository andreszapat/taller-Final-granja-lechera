using Granja.Domain.Exceptions;
namespace Granja.Domain.Entities;
public sealed class Granja
{
    private Granja() { }
    public Granja(int numeroVacas, decimal litrosPorVaca) { Id = 1; Configurar(numeroVacas, litrosPorVaca); }
    public int Id { get; private set; }
    public int NumeroVacas { get; private set; }
    public decimal LitrosPorVaca { get; private set; }
    public void Configurar(int numeroVacas, decimal litrosPorVaca)
    {
        if (numeroVacas <= 0 || numeroVacas > 10000 || litrosPorVaca <= 0)
            throw new ValidacionNegocioException("El número de vacas debe estar entre 1 y 10000, y los litros por vaca deben ser positivos.");
        NumeroVacas = numeroVacas; LitrosPorVaca = litrosPorVaca;
    }
}
