namespace Granja.Domain.Exceptions;
public class ValidacionNegocioException(string mensaje) : Exception(mensaje);
public class ConflictoNegocioException(string mensaje) : Exception(mensaje);
public class RecursoNoEncontradoException(string mensaje) : Exception(mensaje);
public sealed class InventarioInsuficienteException(string producto, decimal cantidad)
    : ConflictoNegocioException($"Inventario insuficiente de {producto} para vender {cantidad} unidades no vencidas.");
public sealed class LecheInsuficienteException(decimal solicitados, decimal disponibles)
    : ConflictoNegocioException($"Se solicitaron {solicitados} L, pero solo hay {disponibles} L de leche sin transformar en esa fecha.");
