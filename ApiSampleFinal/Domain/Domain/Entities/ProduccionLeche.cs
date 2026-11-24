using Domain.Exceptions;

namespace Domain.Entities
{
    /// <summary>Litros producidos por una vaca en una fecha (RN-01: única por fecha y vaca).</summary>
    public class ProduccionLeche
    {
        public int Id { get; private set; }
        public int VacaId { get; private set; }
        public Vaca? Vaca { get; private set; }
        public DateOnly Fecha { get; private set; }
        public decimal Litros { get; private set; }

        private ProduccionLeche() { }

        public ProduccionLeche(int vacaId, DateOnly fecha, decimal litros)
        {
            if (litros <= 0) throw new DomainException("Los litros producidos deben ser mayores que cero.");
            VacaId = vacaId;
            Fecha = fecha;
            Litros = litros;
        }
    }
}
