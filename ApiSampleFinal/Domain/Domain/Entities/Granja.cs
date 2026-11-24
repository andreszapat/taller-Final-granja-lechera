using Domain.Exceptions;

namespace Domain.Entities
{
    /// <summary>Configuración de la granja: n vacas que producen l litros cada una por día.</summary>
    public class Granja
    {
        public int Id { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        /// <summary>n: número de vacas.</summary>
        public int NumeroVacas { get; private set; }
        /// <summary>l: litros por vaca por día.</summary>
        public decimal LitrosPorVaca { get; private set; }

        private Granja() { }

        public Granja(string nombre, int numeroVacas, decimal litrosPorVaca)
        {
            Renombrar(nombre);
            Actualizar(numeroVacas, litrosPorVaca);
        }

        /// <summary>Producción diaria esperada: n × l.</summary>
        public decimal ProduccionDiariaEsperada => NumeroVacas * LitrosPorVaca;

        public void Actualizar(int numeroVacas, decimal litrosPorVaca)
        {
            if (numeroVacas <= 0) throw new DomainException("El número de vacas (n) debe ser mayor que cero.");
            if (litrosPorVaca <= 0) throw new DomainException("Los litros por vaca (l) deben ser mayores que cero.");
            NumeroVacas = numeroVacas;
            LitrosPorVaca = litrosPorVaca;
        }

        public void Renombrar(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new DomainException("El nombre de la granja es obligatorio.");
            Nombre = nombre.Trim();
        }
    }
}
