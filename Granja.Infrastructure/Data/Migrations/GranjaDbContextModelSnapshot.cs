using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace Granja.Infrastructure.Data.Migrations;
[DbContext(typeof(GranjaDbContext))]
public sealed class GranjaDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity<Granja.Domain.Entities.Granja>(e =>
        {
            e.ToTable("Granjas"); e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.LitrosPorVaca).HasColumnType("TEXT");
        });
        modelBuilder.Entity<Vaca>(e =>
        {
            e.ToTable("Vacas"); e.HasKey(x => x.Id);
            e.Property(x => x.Codigo).IsRequired().HasMaxLength(30);
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(120);
            e.HasIndex(x => x.Codigo).IsUnique();
        });
        modelBuilder.Entity<ConfiguracionProducto>(e =>
        {
            e.ToTable("ConfiguracionesProducto"); e.HasKey(x => x.Tipo); e.Property(x => x.Tipo).ValueGeneratedNever();
            e.Property(x => x.RendimientoLitrosPorUnidad).HasColumnType("TEXT");
            e.Property(x => x.PrecioUnitario).HasColumnType("TEXT");
        });
        modelBuilder.Entity<ProduccionLeche>(e =>
        {
            e.ToTable("Producciones"); e.HasKey(x => x.Id);
            e.Property(x => x.Litros).HasColumnType("TEXT");
            e.HasIndex(x => new { x.Fecha, x.VacaId }).IsUnique();
            e.HasOne(x => x.Vaca).WithMany().HasForeignKey(x => x.VacaId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Transformacion>(e =>
        {
            e.ToTable("Transformaciones"); e.HasKey(x => x.Id);
            e.Property(x => x.LitrosUsados).HasColumnType("TEXT");
            e.Property(x => x.CantidadObtenida).HasColumnType("TEXT");
            e.HasOne(x => x.Lote).WithOne(x => x.Transformacion)
                .HasForeignKey<LoteProducto>(x => x.TransformacionId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<LoteProducto>(e =>
        {
            e.ToTable("Lotes"); e.HasKey(x => x.Id);
            e.Property(x => x.CantidadInicial).HasColumnType("TEXT");
            e.Property(x => x.CantidadDisponible).HasColumnType("TEXT");
            e.Property(x => x.CantidadDanada).HasColumnType("TEXT");
            e.Property(x => x.ConcurrencyToken).IsConcurrencyToken();
            e.HasIndex(x => new { x.Tipo, x.FechaProduccion, x.Id });
        });
        modelBuilder.Entity<Venta>(e =>
        {
            e.ToTable("Ventas"); e.HasKey(x => x.Id);
            e.Property(x => x.Cantidad).HasColumnType("TEXT");
            e.Property(x => x.PrecioUnitario).HasColumnType("TEXT");
            e.Property(x => x.Total).HasColumnType("TEXT");
            e.HasMany(x => x.Detalles).WithOne(x => x.Venta).HasForeignKey(x => x.VentaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<VentaDetalle>(e =>
        {
            e.ToTable("VentaDetalles"); e.HasKey(x => x.Id);
            e.Property(x => x.Cantidad).HasColumnType("TEXT");
            e.HasOne(x => x.Lote).WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.Restrict);
        });
        }
}
