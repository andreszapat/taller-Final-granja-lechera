using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure
{
    /// <summary>
    /// Unidad de trabajo: SaveChangesAsync de EF Core envía todos los cambios pendientes
    /// (venta, detalles y lotes descontados) dentro de una sola transacción.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GranjaDbContext _context;
        public UnitOfWork(GranjaDbContext context) => _context = context;

        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    }

    /// <summary>Reloj del sistema (fecha local).</summary>
    public class SystemClock : IClock
    {
        public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
    }
}
