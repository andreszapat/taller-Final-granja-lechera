using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities
{
    /// <summary>Parámetros configurables de un producto: rendimiento, vida útil y precio.</summary>
    public class ProductoParametro
    {
        public int Id { get; private set; }
        public TipoProducto Tipo { get; private set; }
        /// <summary>Unidad de medida del producto (kg, L).</summary>
        public string Unidad { get; private set; } = string.Empty;
        /// <summary>Rendimiento: litros de leche necesarios para una unidad de producto.</summary>
        public decimal LitrosPorUnidad { get; private set; }
        /// <summary>Días entre producción y vencimiento.</summary>
        public int VidaUtilDias { get; private set; }
        /// <summary>Precio de venta por unidad.</summary>
        public decimal Precio { get; private set; }

        private ProductoParametro() { }

        public ProductoParametro(TipoProducto tipo, string unidad, decimal litrosPorUnidad, int vidaUtilDias, decimal precio)
        {
            Tipo = tipo;
            Actualizar(unidad, litrosPorUnidad, vidaUtilDias, precio);
        }

        public void Actualizar(string unidad, decimal litrosPorUnidad, int vidaUtilDias, decimal precio)
        {
            if (string.IsNullOrWhiteSpace(unidad)) throw new DomainException($"La unidad de {Tipo} es obligatoria.");
            if (litrosPorUnidad <= 0) throw new DomainException($"El rendimiento de {Tipo} debe ser mayor que cero.");
            if (vidaUtilDias <= 0) throw new DomainException($"La vida útil de {Tipo} debe ser mayor que cero.");
            if (precio < 0) throw new DomainException($"El precio de {Tipo} no puede ser negativo.");
            Unidad = unidad.Trim();
            LitrosPorUnidad = litrosPorUnidad;
            VidaUtilDias = vidaUtilDias;
            Precio = precio;
        }

        /// <summary>RN-03: cantidad obtenida = litros usados ÷ rendimiento.</summary>
        public decimal CalcularCantidad(decimal litros) => Math.Round(litros / LitrosPorUnidad, 3);

        /// <summary>RN-04: vencimiento = producción + vida útil.</summary>
        public DateOnly CalcularVencimiento(DateOnly fechaProduccion) => fechaProduccion.AddDays(VidaUtilDias);
    }
}
