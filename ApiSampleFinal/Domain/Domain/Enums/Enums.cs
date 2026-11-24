namespace Domain.Enums
{
    /// <summary>Productos que se pueden elaborar con la leche del día.</summary>
    public enum TipoProducto
    {
        Queso = 1,
        Yogurt = 2
    }

    /// <summary>Estado de un lote evaluado en una fecha dada.</summary>
    public enum EstadoLote
    {
        /// <summary>Tiene existencias y no está vencido: se puede vender.</summary>
        Disponible = 1,
        /// <summary>Se vendió por completo.</summary>
        Agotado = 2,
        /// <summary>Venció con existencias que aún no se han procesado como dañadas.</summary>
        Vencido = 3,
        /// <summary>Venció y sus existencias se pasaron a producto dañado.</summary>
        Danado = 4
    }
}
