using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Granja.Infrastructure.Repositories;
public sealed class SystemClock : IClock { public DateOnly Today => DateOnly.FromDateTime(DateTime.Now); }
public sealed class UnitOfWork(GranjaDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
public sealed class GranjaRepository(GranjaDbContext db) : IGranjaRepository
{
    public Task<Granja.Domain.Entities.Granja?> ObtenerAsync(CancellationToken ct = default) => db.Granjas.SingleOrDefaultAsync(ct);
}
public sealed class VacaRepository(GranjaDbContext db) : IVacaRepository
{
    public Task<List<Vaca>> ListarAsync(CancellationToken ct = default) => db.Vacas.OrderBy(x => x.Id).ToListAsync(ct);
    public Task<Vaca?> ObtenerAsync(int id, CancellationToken ct = default) => db.Vacas.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task AgregarAsync(Vaca vaca, CancellationToken ct = default) { await db.Vacas.AddAsync(vaca, ct); }
}
public sealed class ConfiguracionProductoRepository(GranjaDbContext db) : IConfiguracionProductoRepository
{
    public Task<List<ConfiguracionProducto>> ListarAsync(CancellationToken ct = default) => db.ConfiguracionesProducto.OrderBy(x => x.Tipo).ToListAsync(ct);
    public Task<ConfiguracionProducto?> ObtenerAsync(TipoProducto tipo, CancellationToken ct = default) => db.ConfiguracionesProducto.SingleOrDefaultAsync(x => x.Tipo == tipo, ct);
}
public sealed class ProduccionRepository(GranjaDbContext db) : IProduccionRepository
{
    public Task<List<ProduccionLeche>> ListarAsync(DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default)
    {
        var q = db.Producciones.Include(x => x.Vaca).AsQueryable();
        if (desde.HasValue) q = q.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue) q = q.Where(x => x.Fecha <= hasta.Value);
        return q.OrderBy(x => x.Fecha).ThenBy(x => x.VacaId).ToListAsync(ct);
    }
    public async Task AgregarVariasAsync(IEnumerable<ProduccionLeche> producciones, CancellationToken ct = default)
        => await db.Producciones.AddRangeAsync(producciones, ct);
}
public sealed class TransformacionRepository(GranjaDbContext db) : ITransformacionRepository
{
    public Task<List<Transformacion>> ListarAsync(DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default)
    {
        var q = db.Transformaciones.AsQueryable();
        if (desde.HasValue) q = q.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue) q = q.Where(x => x.Fecha <= hasta.Value);
        return q.OrderBy(x => x.Fecha).ThenBy(x => x.Id).ToListAsync(ct);
    }
    public async Task AgregarAsync(Transformacion transformacion, CancellationToken ct = default)
        => await db.Transformaciones.AddAsync(transformacion, ct);
}
public sealed class LoteRepository(GranjaDbContext db) : ILoteRepository
{
    public Task<List<LoteProducto>> ListarAsync(CancellationToken ct = default)
        => db.Lotes.OrderBy(x => x.FechaProduccion).ThenBy(x => x.Id).ToListAsync(ct);
    public async Task<List<LoteProducto>> ObtenerVendiblesAsync(TipoProducto tipo, DateOnly fecha, CancellationToken ct = default)
    {
        // SQLite no traduce de forma fiable comparaciones sobre decimal almacenado como TEXT.
        var lotes = await db.Lotes.Where(x => x.Tipo == tipo && x.FechaProduccion <= fecha && x.FechaVencimiento >= fecha)
            .OrderBy(x => x.FechaProduccion).ThenBy(x => x.Id).ToListAsync(ct);
        return lotes.Where(x => x.CantidadDisponible > 0).ToList();
    }
    public async Task<List<LoteProducto>> ObtenerVencidosAsync(DateOnly fecha, CancellationToken ct = default)
    {
        var lotes = await db.Lotes.Where(x => x.FechaVencimiento < fecha)
            .OrderBy(x => x.FechaVencimiento).ThenBy(x => x.Id).ToListAsync(ct);
        return lotes.Where(x => x.CantidadDisponible > 0).ToList();
    }
}
public sealed class VentaRepository(GranjaDbContext db) : IVentaRepository
{
    public Task<List<Venta>> ListarAsync(DateOnly? desde = null, DateOnly? hasta = null, TipoProducto? tipo = null, CancellationToken ct = default)
    {
        var q = db.Ventas.Include(x => x.Detalles).AsQueryable();
        if (desde.HasValue) q = q.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue) q = q.Where(x => x.Fecha <= hasta.Value);
        if (tipo.HasValue) q = q.Where(x => x.Tipo == tipo.Value);
        return q.OrderBy(x => x.Fecha).ThenBy(x => x.Id).ToListAsync(ct);
    }
    public async Task AgregarAsync(Venta venta, CancellationToken ct = default)
        => await db.Ventas.AddAsync(venta, ct);
}
