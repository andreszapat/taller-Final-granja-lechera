using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialGranja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Granjas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    NumeroVacas = table.Column<int>(type: "INTEGER", nullable: false),
                    LitrosPorVaca = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Granjas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FechaProduccion = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    LitrosUsados = table.Column<double>(type: "REAL", nullable: false),
                    CantidadInicial = table.Column<double>(type: "REAL", nullable: false),
                    CantidadDisponible = table.Column<double>(type: "REAL", nullable: false),
                    CantidadDanada = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductoParametros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Unidad = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    LitrosPorUnidad = table.Column<double>(type: "REAL", nullable: false),
                    VidaUtilDias = table.Column<int>(type: "INTEGER", nullable: false),
                    Precio = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoParametros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vacas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Raza = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    TipoProducto = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Cantidad = table.Column<double>(type: "REAL", nullable: false),
                    PrecioUnitario = table.Column<double>(type: "REAL", nullable: false),
                    Total = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProduccionesLeche",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VacaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Litros = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduccionesLeche", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProduccionesLeche_Vacas_VacaId",
                        column: x => x.VacaId,
                        principalTable: "Vacas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VentaDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cantidad = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Granjas",
                columns: new[] { "Id", "LitrosPorVaca", "Nombre", "NumeroVacas" },
                values: new object[] { 1, 15.0, "Granja Lechera USC", 10 });

            migrationBuilder.InsertData(
                table: "ProductoParametros",
                columns: new[] { "Id", "LitrosPorUnidad", "Precio", "Tipo", "Unidad", "VidaUtilDias" },
                values: new object[,]
                {
                    { 1, 10.0, 28000.0, "Queso", "kg", 30 },
                    { 2, 1.0, 6000.0, "Yogurt", "L", 15 }
                });

            migrationBuilder.InsertData(
                table: "Vacas",
                columns: new[] { "Id", "Activa", "Codigo", "Raza" },
                values: new object[,]
                {
                    { 1, true, "V-001", "Holstein" },
                    { 2, true, "V-002", "Holstein" },
                    { 3, true, "V-003", "Holstein" },
                    { 4, true, "V-004", "Holstein" },
                    { 5, true, "V-005", "Holstein" },
                    { 6, true, "V-006", "Holstein" },
                    { 7, true, "V-007", "Holstein" },
                    { 8, true, "V-008", "Holstein" },
                    { 9, true, "V-009", "Holstein" },
                    { 10, true, "V-010", "Holstein" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_Tipo_FechaProduccion",
                table: "Lotes",
                columns: new[] { "Tipo", "FechaProduccion" });

            migrationBuilder.CreateIndex(
                name: "IX_ProduccionesLeche_Fecha_VacaId",
                table: "ProduccionesLeche",
                columns: new[] { "Fecha", "VacaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProduccionesLeche_VacaId",
                table: "ProduccionesLeche",
                column: "VacaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoParametros_Tipo",
                table: "ProductoParametros",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vacas_Codigo",
                table: "Vacas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_LoteId",
                table: "VentaDetalles",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_VentaId",
                table: "VentaDetalles",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Fecha",
                table: "Ventas",
                column: "Fecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Granjas");

            migrationBuilder.DropTable(
                name: "ProduccionesLeche");

            migrationBuilder.DropTable(
                name: "ProductoParametros");

            migrationBuilder.DropTable(
                name: "VentaDetalles");

            migrationBuilder.DropTable(
                name: "Vacas");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "Ventas");
        }
    }
}
