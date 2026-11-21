using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Granja.Infrastructure.Data.Migrations;
[DbContext(typeof(GranjaDbContext))]
[Migration("20261120000000_Inicial")]
public sealed class Inicial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE "Granjas" ("Id" INTEGER NOT NULL PRIMARY KEY, "NumeroVacas" INTEGER NOT NULL, "LitrosPorVaca" TEXT NOT NULL);
            CREATE TABLE "Vacas" ("Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, "Codigo" TEXT NOT NULL, "Nombre" TEXT NOT NULL, "Activa" INTEGER NOT NULL);
            CREATE UNIQUE INDEX "IX_Vacas_Codigo" ON "Vacas" ("Codigo");
            CREATE TABLE "ConfiguracionesProducto" ("Tipo" INTEGER NOT NULL PRIMARY KEY, "RendimientoLitrosPorUnidad" TEXT NOT NULL, "VidaUtilDias" INTEGER NOT NULL, "PrecioUnitario" TEXT NOT NULL);
            CREATE TABLE "Producciones" ("Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, "VacaId" INTEGER NOT NULL, "Fecha" TEXT NOT NULL, "Litros" TEXT NOT NULL, FOREIGN KEY ("VacaId") REFERENCES "Vacas" ("Id") ON DELETE RESTRICT);
            CREATE UNIQUE INDEX "IX_Producciones_Fecha_VacaId" ON "Producciones" ("Fecha", "VacaId");
            CREATE INDEX "IX_Producciones_VacaId" ON "Producciones" ("VacaId");
            CREATE TABLE "Transformaciones" ("Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, "Tipo" INTEGER NOT NULL, "Fecha" TEXT NOT NULL, "LitrosUsados" TEXT NOT NULL, "CantidadObtenida" TEXT NOT NULL);
            CREATE TABLE "Lotes" ("Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, "TransformacionId" INTEGER NOT NULL, "Tipo" INTEGER NOT NULL, "FechaProduccion" TEXT NOT NULL, "FechaVencimiento" TEXT NOT NULL, "CantidadInicial" TEXT NOT NULL, "CantidadDisponible" TEXT NOT NULL, "CantidadDanada" TEXT NOT NULL, "FechaProcesoDanado" TEXT NULL, "ConcurrencyToken" TEXT NOT NULL, FOREIGN KEY ("TransformacionId") REFERENCES "Transformaciones" ("Id") ON DELETE RESTRICT);
            CREATE UNIQUE INDEX "IX_Lotes_TransformacionId" ON "Lotes" ("TransformacionId");
            CREATE INDEX "IX_Lotes_Tipo_FechaProduccion_Id" ON "Lotes" ("Tipo", "FechaProduccion", "Id");
            CREATE TABLE "Ventas" ("Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, "Fecha" TEXT NOT NULL, "Tipo" INTEGER NOT NULL, "Cantidad" TEXT NOT NULL, "PrecioUnitario" TEXT NOT NULL, "Total" TEXT NOT NULL);
            CREATE TABLE "VentaDetalles" ("Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, "VentaId" INTEGER NOT NULL, "LoteId" INTEGER NOT NULL, "FechaProduccion" TEXT NOT NULL, "Cantidad" TEXT NOT NULL, FOREIGN KEY ("VentaId") REFERENCES "Ventas" ("Id") ON DELETE RESTRICT, FOREIGN KEY ("LoteId") REFERENCES "Lotes" ("Id") ON DELETE RESTRICT);
            CREATE INDEX "IX_VentaDetalles_VentaId" ON "VentaDetalles" ("VentaId");
            CREATE INDEX "IX_VentaDetalles_LoteId" ON "VentaDetalles" ("LoteId");
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS "VentaDetalles";
            DROP TABLE IF EXISTS "Ventas";
            DROP TABLE IF EXISTS "Lotes";
            DROP TABLE IF EXISTS "Transformaciones";
            DROP TABLE IF EXISTS "Producciones";
            DROP TABLE IF EXISTS "ConfiguracionesProducto";
            DROP TABLE IF EXISTS "Vacas";
            DROP TABLE IF EXISTS "Granjas";
            """);
    }
}
