using Domain.Enums;

namespace Domain.Exceptions
{
    /// <summary>Error de regla de negocio o de validación. Se traduce a HTTP 400.</summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }

    /// <summary>La entidad solicitada no existe (HTTP 404).</summary>
    public class EntidadNoEncontradaException : DomainException
    {
        public EntidadNoEncontradaException(string entidad, object id)
            : base($"{entidad} con identificador '{id}' no existe.") { }
    }

    /// <summary>La operación choca con el estado actual de los datos (HTTP 409).</summary>
    public abstract class ConflictoException : DomainException
    {
        protected ConflictoException(string message) : base(message) { }
    }

    /// <summary>Ya existe un registro con el mismo valor único (HTTP 409).</summary>
    public class DatoDuplicadoException : ConflictoException
    {
        public DatoDuplicadoException(string message) : base(message) { }
    }

    /// <summary>RN-07: no hay existencias no vencidas suficientes para la venta.</summary>
    public class InventarioInsuficienteException : ConflictoException
    {
        public TipoProducto Tipo { get; }
        public decimal Solicitado { get; }
        public decimal Disponible { get; }

        public InventarioInsuficienteException(TipoProducto tipo, decimal solicitado, decimal disponible)
            : base($"Inventario insuficiente de {tipo}: se solicitaron {solicitado} y solo hay {disponible} sin vencer.")
        {
            Tipo = tipo;
            Solicitado = solicitado;
            Disponible = disponible;
        }
    }

    /// <summary>RN-02: se intentan transformar más litros de los producidos ese día.</summary>
    public class LecheInsuficienteException : ConflictoException
    {
        public DateOnly Fecha { get; }
        public decimal Solicitado { get; }
        public decimal Disponible { get; }

        public LecheInsuficienteException(DateOnly fecha, decimal solicitado, decimal disponible)
            : base($"Leche insuficiente el {fecha:yyyy-MM-dd}: se pidieron {solicitado} L y quedan {disponible} L sin transformar.")
        {
            Fecha = fecha;
            Solicitado = solicitado;
            Disponible = disponible;
        }
    }

    /// <summary>RN-01: la producción se registra una sola vez por fecha y por vaca.</summary>
    public class ProduccionDuplicadaException : ConflictoException
    {
        public ProduccionDuplicadaException(DateOnly fecha)
            : base($"La producción de leche del {fecha:yyyy-MM-dd} ya fue registrada.") { }

        public ProduccionDuplicadaException(DateOnly fecha, int vacaId)
            : base($"La vaca {vacaId} ya tiene producción registrada el {fecha:yyyy-MM-dd}.") { }
    }
}
