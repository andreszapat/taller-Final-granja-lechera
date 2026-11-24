using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>Repositorio genérico. Solo registra cambios en el contexto; el guardado lo hace la unidad de trabajo.</summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly GranjaDbContext Context;
        protected DbSet<T> Set => Context.Set<T>();

        public Repository(GranjaDbContext context) => Context = context;

        public virtual async Task<T?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
            await Set.FindAsync(new object[] { id }, ct);

        public virtual async Task<IReadOnlyList<T>> ListarAsync(CancellationToken ct = default) =>
            await Set.ToListAsync(ct);

        public async Task AgregarAsync(T entidad, CancellationToken ct = default) =>
            await Set.AddAsync(entidad, ct);
    }

    public class GranjaRepository : IGranjaRepository
    {
        private readonly GranjaDbContext _context;
        public GranjaRepository(GranjaDbContext context) => _context = context;

        public async Task<Granja> ObtenerAsync(CancellationToken ct = default) =>
            await _context.Granjas.OrderBy(g => g.Id).FirstOrDefaultAsync(ct)
            ?? throw new EntidadNoEncontradaException("Granja", 1);

        public async Task<IReadOnlyList<ProductoParametro>> ObtenerParametrosAsync(CancellationToken ct = default) =>
            await _context.ProductoParametros.OrderBy(p => p.Id).ToListAsync(ct);

        public async Task<ProductoParametro> ObtenerParametroAsync(TipoProducto tipo, CancellationToken ct = default) =>
            await _context.ProductoParametros.FirstOrDefaultAsync(p => p.Tipo == tipo, ct)
            ?? throw new EntidadNoEncontradaException("Parámetro de producto", tipo);
    }

    public class VacaRepository : Repository<Vaca>, IVacaRepository
    {
        public VacaRepository(GranjaDbContext context) : base(context) { }

        public override async Task<IReadOnlyList<Vaca>> ListarAsync(CancellationToken ct = default) =>
            await Set.OrderBy(v => v.Id).ToListAsync(ct);

        public async Task<IReadOnlyList<Vaca>> ListarActivasAsync(CancellationToken ct = default) =>
            await Set.Where(v => v.Activa).OrderBy(v => v.Id).ToListAsync(ct);

        public Task<int> ContarAsync(CancellationToken ct = default) => Set.CountAsync(ct);
    }

    public class ProduccionLecheRepository : Repository<ProduccionLeche>, IProduccionLecheRepository
    {
        public ProduccionLecheRepository(GranjaDbContext context) : base(context) { }

        public Task<bool> ExisteAsync(DateOnly fecha, int? vacaId = null, CancellationToken ct = default) =>
            Set.AnyAsync(p => p.Fecha == fecha && (vacaId == null || p.VacaId == vacaId), ct);

        public async Task<decimal> LitrosDelDiaAsync(DateOnly fecha, CancellationToken ct = default) =>
            (await Set.Where(p => p.Fecha == fecha).Select(p => p.Litros).ToListAsync(ct)).Sum();

        public async Task<IReadOnlyList<ProduccionLeche>> ObtenerPorRangoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
            await Set.Include(p => p.Vaca)
                .Where(p => (desde == null || p.Fecha >= desde) && (hasta == null || p.Fecha <= hasta))
                .OrderBy(p => p.Fecha).ThenBy(p => p.VacaId)
                .ToListAsync(ct);
    }

    public class LoteRepository : Repository<LoteProducto>, ILoteRepository
    {
        public LoteRepository(GranjaDbContext context) : base(context) { }

        public async Task<IReadOnlyList<LoteProducto>> ObtenerVendiblesAsync(TipoProducto tipo, DateOnly fecha, CancellationToken ct = default) =>
            await Set.Where(l => l.Tipo == tipo
                              && l.CantidadDisponible > 0
                              && l.FechaProduccion <= fecha
                              && l.FechaVencimiento >= fecha)
                .OrderBy(l => l.FechaProduccion).ThenBy(l => l.Id)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<LoteProducto>> ObtenerVencidosConExistenciasAsync(DateOnly fecha, CancellationToken ct = default) =>
            await Set.Where(l => l.CantidadDisponible > 0 && l.FechaVencimiento < fecha)
                .OrderBy(l => l.FechaProduccion).ThenBy(l => l.Id)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<LoteProducto>> ObtenerPorRangoAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo = null, CancellationToken ct = default) =>
            await Set.Where(l => (desde == null || l.FechaProduccion >= desde)
                              && (hasta == null || l.FechaProduccion <= hasta)
                              && (tipo == null || l.Tipo == tipo))
                .OrderBy(l => l.FechaProduccion).ThenBy(l => l.Id)
                .ToListAsync(ct);

        public async Task<decimal> LitrosTransformadosAsync(DateOnly fecha, CancellationToken ct = default) =>
            (await Set.Where(l => l.FechaProduccion == fecha).Select(l => l.LitrosUsados).ToListAsync(ct)).Sum();
    }

    public class VentaRepository : Repository<Venta>, IVentaRepository
    {
        public VentaRepository(GranjaDbContext context) : base(context) { }

        private IQueryable<Venta> ConDetalle => Set.Include(v => v.Detalles).ThenInclude(d => d.Lote);

        public override async Task<Venta?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
            await ConDetalle.FirstOrDefaultAsync(v => v.Id == id, ct);

        public override async Task<IReadOnlyList<Venta>> ListarAsync(CancellationToken ct = default) =>
            await ConDetalle.OrderBy(v => v.Fecha).ThenBy(v => v.Id).ToListAsync(ct);

        public async Task<IReadOnlyList<Venta>> ObtenerPorRangoAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo = null, CancellationToken ct = default) =>
            await ConDetalle.Where(v => (desde == null || v.Fecha >= desde)
                                     && (hasta == null || v.Fecha <= hasta)
                                     && (tipo == null || v.TipoProducto == tipo))
                .OrderBy(v => v.Fecha).ThenBy(v => v.Id)
                .ToListAsync(ct);
    }
}
