using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities
{
    /// <summary>
    /// Lote del inventario I. Se crea al transformar leche (RN-04) y concentra las reglas
    /// de vencimiento, descuento por venta y paso a producto dañado.
    /// </summary>
    public class LoteProducto
    {
        public int Id { get; private set; }
        public TipoProducto Tipo { get; private set; }
        public DateOnly FechaProduccion { get; private set; }
        public DateOnly FechaVencimiento { get; private set; }
        /// <summary>Litros de leche consumidos para elaborar el lote.</summary>
        public decimal LitrosUsados { get; private set; }
        public decimal CantidadInicial { get; private set; }
        public decimal CantidadDisponible { get; private set; }
        public decimal CantidadDanada { get; private set; }

        private LoteProducto() { }

        /// <summary>RN-03 y RN-04: crea el lote a partir de los litros y los parámetros del producto.</summary>
        public LoteProducto(ProductoParametro parametro, DateOnly fechaProduccion, decimal litrosUsados)
        {
            if (litrosUsados <= 0) throw new DomainException("Los litros a transformar deben ser mayores que cero.");
            var cantidad = parametro.CalcularCantidad(litrosUsados);
            if (cantidad <= 0) throw new DomainException("Los litros indicados no alcanzan para producir una cantidad válida.");

            Tipo = parametro.Tipo;
            FechaProduccion = fechaProduccion;
            FechaVencimiento = parametro.CalcularVencimiento(fechaProduccion);
            LitrosUsados = litrosUsados;
            CantidadInicial = cantidad;
            CantidadDisponible = cantidad;
        }

        /// <summary>RN-08: vencido cuando la fecha es posterior a la de vencimiento.</summary>
        public bool EstaVencido(DateOnly fecha) => fecha > FechaVencimiento;

        /// <summary>Se puede vender en la fecha: ya se produjo, no está vencido y tiene existencias.</summary>
        public bool EsVendible(DateOnly fecha) =>
            FechaProduccion <= fecha && !EstaVencido(fecha) && CantidadDisponible > 0;

        public decimal CantidadVendida => CantidadInicial - CantidadDisponible - CantidadDanada;

        public EstadoLote ObtenerEstado(DateOnly fecha)
        {
            if (CantidadDanada > 0 && CantidadDisponible == 0) return EstadoLote.Danado;
            if (CantidadDisponible == 0) return EstadoLote.Agotado;
            return EstaVencido(fecha) ? EstadoLote.Vencido : EstadoLote.Disponible;
        }

        /// <summary>Lote vendible que vence dentro de los próximos <paramref name="dias"/> días.</summary>
        public bool VenceEnLosProximos(DateOnly fecha, int dias) =>
            CantidadDisponible > 0 && !EstaVencido(fecha) && FechaVencimiento <= fecha.AddDays(dias);

        public void Descontar(decimal cantidad)
        {
            if (cantidad <= 0 || cantidad > CantidadDisponible)
                throw new InvalidOperationException("Cantidad inválida para el lote.");
            CantidadDisponible -= cantidad;
        }

        public decimal MarcarComoDanado()
        {
            var perdida = CantidadDisponible;
            CantidadDanada += perdida;
            CantidadDisponible = 0;
            return perdida;
        }

        /// <summary>RN-08: si el lote está vencido en la fecha, sus existencias pasan a dañadas.</summary>
        public decimal ProcesarVencimiento(DateOnly fecha) =>
            EstaVencido(fecha) && CantidadDisponible > 0 ? MarcarComoDanado() : 0;
    }
}
