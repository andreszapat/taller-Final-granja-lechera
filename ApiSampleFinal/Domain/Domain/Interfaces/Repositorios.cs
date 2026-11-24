using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    /// <summary>RN-10: fuente de la fecha actual, reemplazable en pruebas.</summary>
    public interface IClock
    {
        DateOnly Today { get; }
    }

    /// <summary>Confirma todos los cambios pendientes en una sola transacción.</summary>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }

    /// <summary>Operaciones comunes de acceso a datos. No guarda: eso lo hace <see cref="IUnitOfWork"/>.</summary>
    public interface IRepository<T> where T : class
    {
        Task<T?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<T>> ListarAsync(CancellationToken ct = default);
        Task AgregarAsync(T entidad, CancellationToken ct = default);
    }

    public interface IGranjaRepository
    {
        /// <summary>Devuelve la única granja configurada.</summary>
        Task<Granja> ObtenerAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ProductoParametro>> ObtenerParametrosAsync(CancellationToken ct = default);
        Task<ProductoParametro> ObtenerParametroAsync(TipoProducto tipo, CancellationToken ct = default);
    }

    public interface IVacaRepository : IRepository<Vaca>
    {
        Task<IReadOnlyList<Vaca>> ListarActivasAsync(CancellationToken ct = default);
        Task<int> ContarAsync(CancellationToken ct = default);
    }

    public interface IProduccionLecheRepository : IRepository<ProduccionLeche>
    {
        Task<bool> ExisteAsync(DateOnly fecha, int? vacaId = null, CancellationToken ct = default);
        Task<decimal> LitrosDelDiaAsync(DateOnly fecha, CancellationToken ct = default);
        Task<IReadOnlyList<ProduccionLeche>> ObtenerPorRangoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    }

    public interface ILoteRepository : IRepository<LoteProducto>
    {
        /// <summary>Lotes con existencias, no vencidos a la fecha, ordenados por FechaProduccion ascendente (FIFO).</summary>
        Task<IReadOnlyList<LoteProducto>> ObtenerVendiblesAsync(TipoProducto tipo, DateOnly fecha, CancellationToken ct = default);
        /// <summary>Lotes vencidos a la fecha que aún tienen cantidad disponible.</summary>
        Task<IReadOnlyList<LoteProducto>> ObtenerVencidosConExistenciasAsync(DateOnly fecha, CancellationToken ct = default);
        Task<IReadOnlyList<LoteProducto>> ObtenerPorRangoAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo = null, CancellationToken ct = default);
        Task<decimal> LitrosTransformadosAsync(DateOnly fecha, CancellationToken ct = default);
    }

    public interface IVentaRepository : IRepository<Venta>
    {
        Task<IReadOnlyList<Venta>> ObtenerPorRangoAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo = null, CancellationToken ct = default);
    }
}
