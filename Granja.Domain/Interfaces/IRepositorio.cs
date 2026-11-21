using Granja.Domain.Entities;
using Granja.Domain.Enums;
namespace Granja.Domain.Interfaces;
public interface IClock { DateOnly Today { get; } }
public interface IUnitOfWork { Task<int> SaveChangesAsync(CancellationToken ct = default); }
public interface IGranjaRepository { Task<global::Granja.Domain.Entities.Granja?> ObtenerAsync(CancellationToken ct = default); }
public interface IVacaRepository
{
    Task<List<Vaca>> ListarAsync(CancellationToken ct = default);
    Task<Vaca?> ObtenerAsync(int id, CancellationToken ct = default);
    Task AgregarAsync(Vaca vaca, CancellationToken ct = default);
}
public interface IConfiguracionProductoRepository
{
    Task<List<ConfiguracionProducto>> ListarAsync(CancellationToken ct = default);
    Task<ConfiguracionProducto?> ObtenerAsync(TipoProducto tipo, CancellationToken ct = default);
}
public interface IProduccionRepository
{
    Task<List<ProduccionLeche>> ListarAsync(DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default);
    Task AgregarVariasAsync(IEnumerable<ProduccionLeche> producciones, CancellationToken ct = default);
}
public interface ITransformacionRepository
{
    Task<List<Transformacion>> ListarAsync(DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default);
    Task AgregarAsync(Transformacion transformacion, CancellationToken ct = default);
}
public interface ILoteRepository
{
    Task<List<LoteProducto>> ListarAsync(CancellationToken ct = default);
    Task<List<LoteProducto>> ObtenerVendiblesAsync(TipoProducto tipo, DateOnly fecha, CancellationToken ct = default);
    Task<List<LoteProducto>> ObtenerVencidosAsync(DateOnly fecha, CancellationToken ct = default);
}
public interface IVentaRepository
{
    Task<List<Venta>> ListarAsync(DateOnly? desde = null, DateOnly? hasta = null, TipoProducto? tipo = null, CancellationToken ct = default);
    Task AgregarAsync(Venta venta, CancellationToken ct = default);
}
