using Domain.Exceptions;

namespace Domain.Entities
{
    /// <summary>Vaca de la granja. Su leche diaria se registra en <see cref="ProduccionLeche"/>.</summary>
    public class Vaca
    {
        public int Id { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Raza { get; private set; } = string.Empty;
        public bool Activa { get; private set; } = true;

        private readonly List<ProduccionLeche> _producciones = new();
        public IReadOnlyCollection<ProduccionLeche> Producciones => _producciones;

        private Vaca() { }

        public Vaca(string codigo, string raza)
        {
            if (string.IsNullOrWhiteSpace(codigo)) throw new DomainException("El código de la vaca es obligatorio.");
            Codigo = codigo.Trim();
            Raza = (raza ?? string.Empty).Trim();
        }

        public void Desactivar() => Activa = false;
        public void Activar() => Activa = true;
    }
}
