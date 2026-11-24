using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class GranjaConfiguration : IEntityTypeConfiguration<Granja>
    {
        public void Configure(EntityTypeBuilder<Granja> b)
        {
            b.ToTable("Granjas");
            b.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            b.Ignore(x => x.ProduccionDiariaEsperada);
            // Valores sugeridos del enunciado; se cambian con PUT /api/granja.
            b.HasData(new { Id = 1, Nombre = "Granja Lechera USC", NumeroVacas = 10, LitrosPorVaca = 15m });
        }
    }

    public class ProductoParametroConfiguration : IEntityTypeConfiguration<ProductoParametro>
    {
        public void Configure(EntityTypeBuilder<ProductoParametro> b)
        {
            b.ToTable("ProductoParametros");
            b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(x => x.Tipo).IsUnique();
            b.Property(x => x.Unidad).HasMaxLength(10).IsRequired();
            b.HasData(
                new { Id = 1, Tipo = TipoProducto.Queso, Unidad = "kg", LitrosPorUnidad = 10m, VidaUtilDias = 30, Precio = 28000m },
                new { Id = 2, Tipo = TipoProducto.Yogurt, Unidad = "L", LitrosPorUnidad = 1m, VidaUtilDias = 15, Precio = 6000m });
        }
    }

    public class VacaConfiguration : IEntityTypeConfiguration<Vaca>
    {
        public void Configure(EntityTypeBuilder<Vaca> b)
        {
            b.ToTable("Vacas");
            b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.Codigo).IsUnique();
            b.Property(x => x.Raza).HasMaxLength(60);
            b.HasMany(x => x.Producciones).WithOne(p => p.Vaca).HasForeignKey(p => p.VacaId);
            b.Navigation(x => x.Producciones).UsePropertyAccessMode(PropertyAccessMode.Field);
            b.HasData(Enumerable.Range(1, 10)
                .Select(i => new { Id = i, Codigo = $"V-{i:000}", Raza = "Holstein", Activa = true })
                .ToArray());
        }
    }

    public class ProduccionLecheConfiguration : IEntityTypeConfiguration<ProduccionLeche>
    {
        public void Configure(EntityTypeBuilder<ProduccionLeche> b)
        {
            b.ToTable("ProduccionesLeche");
            // RN-01: una sola producción por fecha y por vaca.
            b.HasIndex(x => new { x.Fecha, x.VacaId }).IsUnique();
        }
    }

    public class LoteProductoConfiguration : IEntityTypeConfiguration<LoteProducto>
    {
        public void Configure(EntityTypeBuilder<LoteProducto> b)
        {
            b.ToTable("Lotes");
            b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
            b.Ignore(x => x.CantidadVendida);
            b.HasIndex(x => new { x.Tipo, x.FechaProduccion });
        }
    }

    public class VentaConfiguration : IEntityTypeConfiguration<Venta>
    {
        public void Configure(EntityTypeBuilder<Venta> b)
        {
            b.ToTable("Ventas");
            b.Property(x => x.TipoProducto).HasConversion<string>().HasMaxLength(20);
            b.HasMany(x => x.Detalles).WithOne().HasForeignKey(d => d.VentaId).OnDelete(DeleteBehavior.Cascade);
            b.Navigation(x => x.Detalles).UsePropertyAccessMode(PropertyAccessMode.Field);
            b.HasIndex(x => x.Fecha);
        }
    }

    public class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
    {
        public void Configure(EntityTypeBuilder<VentaDetalle> b)
        {
            b.ToTable("VentaDetalles");
            b.HasOne(x => x.Lote).WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
