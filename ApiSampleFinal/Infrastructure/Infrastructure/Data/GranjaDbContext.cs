using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class GranjaDbContext : DbContext
    {
        public GranjaDbContext(DbContextOptions<GranjaDbContext> options) : base(options)
        {
        }

        public DbSet<Granja> Granjas => Set<Granja>();
        public DbSet<ProductoParametro> ProductoParametros => Set<ProductoParametro>();
        public DbSet<Vaca> Vacas => Set<Vaca>();
        public DbSet<ProduccionLeche> ProduccionesLeche => Set<ProduccionLeche>();
        public DbSet<LoteProducto> Lotes => Set<LoteProducto>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<VentaDetalle> VentaDetalles => Set<VentaDetalle>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GranjaDbContext).Assembly);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // SQLite no tiene tipo decimal nativo: se guarda como REAL para poder filtrar,
            // ordenar y sumar en la base de datos. En SQL Server se usa decimal(18,3).
            if (Database.IsSqlite())
                configurationBuilder.Properties<decimal>().HaveConversion<double>();
            else
                configurationBuilder.Properties<decimal>().HavePrecision(18, 3);
        }
    }
}
