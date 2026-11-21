using Granja.Domain.Exceptions;
namespace Granja.Domain.Entities;
public sealed class Vaca
{
    private Vaca() { }
    public Vaca(string codigo, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
            throw new ValidacionNegocioException("Cada vaca debe tener un código y un nombre.");
        Codigo = codigo; Nombre = nombre; Activa = true;
    }
    public int Id { get; private set; }
    public string Codigo { get; private set; } = "";
    public string Nombre { get; private set; } = "";
    public bool Activa { get; private set; }
    public void Activar() => Activa = true;
    public void Desactivar() => Activa = false;
    public void Renombrar(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ValidacionNegocioException("El nombre no puede estar vacío.");
        Nombre = nombre.Trim();
    }
}
