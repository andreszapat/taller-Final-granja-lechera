using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace Granja.Infrastructure.Data;
public static class DbSeeder
{
    public static async Task InicializarAsync(GranjaDbContext db, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);
        if (await db.Granjas.AnyAsync(ct)) return;
        db.Granjas.Add(new Granja.Domain.Entities.Granja(10, 15m));
        db.ConfiguracionesProducto.AddRange(
            new ConfiguracionProducto(TipoProducto.Queso, 10m, 30, 28000m),
            new ConfiguracionProducto(TipoProducto.Yogurt, 1m, 15, 6000m));
        for (var i = 1; i <= 10; i++) db.Vacas.Add(new Vaca($"V{i:0000}", $"Vaca {i}"));
        await db.SaveChangesAsync(ct);
    }
}
