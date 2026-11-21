using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Granja.Infrastructure.Repositories;
using Granja.Services.Dtos;
using Granja.Services.Mapping;
using Granja.Services.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace Granja.Tests;

public sealed class RelojPrueba(DateOnly hoy) : IClock { public DateOnly Today { get; set; } = hoy; }

public sealed class ReglasNegocioTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private GranjaDbContext _db = null!;
    private ProduccionService _produccion = null!;
    private VentaService _venta = null!;
    private InventarioService _inventario = null!;
    private ReporteService _reportes = null!;
    private VacaService _vacas = null!;
    private readonly DateOnly _d1 = new(2026, 12, 1);
    private readonly DateOnly _d2 = new(2026, 12, 2);
    private readonly DateOnly _d3 = new(2026, 12, 3);
    private readonly DateOnly _d17 = new(2026, 12, 17);
    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<GranjaDbContext>().UseSqlite(_connection).Options;
        _db = new GranjaDbContext(options);
        await _db.Database.EnsureCreatedAsync();
        _db.Granjas.Add(new Granja.Domain.Entities.Granja(10, 15));
        _db.ConfiguracionesProducto.AddRange(
            new ConfiguracionProducto(TipoProducto.Queso, 10, 30, 28000),
            new ConfiguracionProducto(TipoProducto.Yogurt, 1, 15, 6000));
        for (var i = 1; i <= 10; i++) _db.Vacas.Add(new Vaca($"V{i:0000}", $"Vaca {i}"));
        await _db.SaveChangesAsync();
        var granja = new GranjaRepository(_db);
        var vacas = new VacaRepository(_db);
        var configs = new ConfiguracionProductoRepository(_db);
        var leche = new ProduccionRepository(_db);
        var transformaciones = new TransformacionRepository(_db);
        var lotes = new LoteRepository(_db);
        var ventas = new VentaRepository(_db);
        var uow = new UnitOfWork(_db);
        var reloj = new RelojPrueba(_d1);
        var mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>(),
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance).CreateMapper();
        _produccion = new ProduccionService(granja, vacas, leche, transformaciones, configs, reloj, uow);
        _venta = new VentaService(lotes, ventas, configs, uow, reloj);
        _inventario = new InventarioService(lotes, reloj, uow, mapper);
        _reportes = new ReporteService(leche, transformaciones, lotes, ventas, vacas, reloj);
        _vacas = new VacaService(vacas, uow, mapper);
    }
    public async Task DisposeAsync() { await _db.DisposeAsync(); await _connection.DisposeAsync(); }
    private Task<ProduccionResponse> Producir(DateOnly fecha)
        => _produccion.RegistrarLecheAsync(new ProduccionRequest(fecha, null));
    private Task<TransformacionResponse> Transformar(TipoProducto tipo, decimal litros, DateOnly fecha)
        => _produccion.TransformarAsync(new TransformacionRequest(tipo, litros, fecha));
    private Task<VentaResponse> Vender(TipoProducto tipo, decimal unidades, DateOnly fecha)
        => _venta.RegistrarVentaAsync(new VentaRequest(tipo, unidades, fecha));
    private async Task PrepararQuesosAsync()
    {
        await Producir(_d1); await Transformar(TipoProducto.Queso, 100, _d1);
        await Producir(_d2); await Transformar(TipoProducto.Queso, 150, _d2);
    }
    [Fact]
    public async Task ProduccionEsNXLYNoSeDuplica()
    {
        var r = await Producir(_d1);
        Assert.Equal(150m, r.TotalLitros);
        Assert.Equal(10, r.Vacas.Count);
        await Assert.ThrowsAsync<ConflictoNegocioException>(() => Producir(_d1));
        Assert.Equal(10, await _db.Producciones.CountAsync());
    }
    [Fact]
    public async Task ProduccionPorVacaEsUnicaYDebeSumarNxL()
    {
        var vacas = await _vacas.ListarAsync();
        var entradas = vacas.Select((x, i) => new ProduccionVacaRequest(x.Id, i == 0 ? 16 : i == 1 ? 14 : 15)).ToList();
        var r = await _produccion.RegistrarLecheAsync(new ProduccionRequest(_d1, entradas));
        Assert.Equal(150m, r.TotalLitros);
        var reporte = await _reportes.VacasAsync();
        Assert.Equal(15m, reporte.PromedioGeneralPorVaca);
        Assert.Equal(10, reporte.Vacas.Count);
        Assert.Single(reporte.MayorProduccion);
        Assert.Equal("V0001", reporte.MayorProduccion[0].Codigo);
        Assert.Equal(16m, reporte.MayorProduccion[0].PromedioLitrosPorDia);
        Assert.Single(reporte.MenorProduccion);
        Assert.Equal("V0002", reporte.MenorProduccion[0].Codigo);
        Assert.Equal(14m, reporte.MenorProduccion[0].PromedioLitrosPorDia);
        Assert.Equal("V0002", Assert.Single(reporte.PorDebajoDelPromedio).Codigo);
    }
    [Fact]
    public async Task NoSePuedeTransformarLecheInexistente()
    {
        await Producir(_d1);
        await Assert.ThrowsAsync<LecheInsuficienteException>(() => Transformar(TipoProducto.Queso, 151, _d1));
        await Transformar(TipoProducto.Queso, 100, _d1);
        await Assert.ThrowsAsync<LecheInsuficienteException>(() => Transformar(TipoProducto.Yogurt, 51, _d1));
        Assert.Single(await _db.Transformaciones.ToListAsync());
    }
    [Fact]
    public async Task SeConsumePrimeroElLoteMasAntiguoYSeDividenLosDetalles()
    {
        await PrepararQuesosAsync();
        var venta = await Vender(TipoProducto.Queso, 12, _d3);
        Assert.Equal(2, venta.Detalle.Count);
        Assert.Equal(10m, venta.Detalle[0].Cantidad);
        Assert.Equal(_d1, venta.Detalle[0].FechaProduccion);
        Assert.Equal(2m, venta.Detalle[1].Cantidad);
        Assert.Equal(_d2, venta.Detalle[1].FechaProduccion);
        Assert.Equal(336000m, venta.Total);
        var q = (await _inventario.ListarAsync(TipoProducto.Queso, _d3)).OrderBy(x => x.FechaProduccion).ToList();
        Assert.Equal(0m, q[0].CantidadDisponible);
        Assert.Equal(13m, q[1].CantidadDisponible);
    }
    [Fact]
    public async Task LoteVencidoNoEsVendible()
    {
        await Producir(_d1);
        await Transformar(TipoProducto.Queso, 100, _d1);
        var d20 = new DateOnly(2026, 12, 20);
        await Producir(d20);
        await Transformar(TipoProducto.Queso, 50, d20);
        var venta = await Vender(TipoProducto.Queso, 2, new DateOnly(2027, 1, 5));
        Assert.Single(venta.Detalle);
        Assert.Equal(d20, venta.Detalle[0].FechaProduccion);
    }
    [Fact]
    public async Task VentaInsuficienteNoDescuentaNiInsertaNada()
    {
        await PrepararQuesosAsync();
        await Assert.ThrowsAsync<InventarioInsuficienteException>(() => Vender(TipoProducto.Queso, 26, _d3));
        var stock = await _inventario.ListarAsync(TipoProducto.Queso, _d3);
        Assert.Equal(25m, stock.Sum(x => x.CantidadDisponible));
        Assert.Empty(await _db.Ventas.ToListAsync());
    }
    [Fact]
    public async Task AlProcesarVencimientosSoloDaniaLoQueNoSeVendio()
    {
        await Producir(_d1);
        await Transformar(TipoProducto.Yogurt, 50, _d1);
        await Vender(TipoProducto.Yogurt, 20, _d3);
        var r = await _inventario.ProcesarVencimientosAsync(new VencimientoRequest(_d17));
        Assert.Equal(30m, r.UnidadesDanadas);
        Assert.Equal(1, r.LotesProcesados);
        var otra = await _inventario.ProcesarVencimientosAsync(new VencimientoRequest(_d17));
        Assert.Equal(0m, otra.UnidadesDanadas);
        Assert.Equal(30m, Assert.Single(await _reportes.ProductoDanadoAsync()).CantidadDanada);
        await Assert.ThrowsAsync<InventarioInsuficienteException>(() => Vender(TipoProducto.Yogurt, 5, _d17));
    }
    [Fact]
    public async Task ReportesDeVentaProductoDiarioYLecheNoAprovechada()
    {
        await Producir(_d1);
        await Transformar(TipoProducto.Queso, 100, _d1);
        await Vender(TipoProducto.Queso, 3, _d3);
        var ventas = await _reportes.VentasAsync(_d1, _d3, TipoProducto.Queso);
        Assert.Equal(84000m, ventas.TotalGeneral);
        Assert.Equal(3m, Assert.Single(ventas.Ventas).Cantidad);
        var diario = await _reportes.ProductoDiarioAsync(_d1, _d3);
        Assert.Equal(10m, Assert.Single(diario).CantidadProducida);
        var leche = await _reportes.LecheDiariaAsync(_d1, _d3);
        Assert.Equal(50m, Assert.Single(leche).LitrosNoAprovechados);
    }
    [Fact]
    public async Task NoAceptaProduccionParcialNiProductosInvalidos()
    {
        var vacas = await _vacas.ListarAsync();
        await Assert.ThrowsAsync<ValidacionNegocioException>(() =>
            _produccion.RegistrarLecheAsync(new ProduccionRequest(_d1,
                new List<ProduccionVacaRequest> { new(vacas[0].Id, 150) })));
        await Assert.ThrowsAsync<ValidacionNegocioException>(() =>
            Transformar((TipoProducto)999, 20, _d1));
    }
    [Fact]
    public async Task EscenarioDeReferenciaCoincideConLaGuia()
    {
        await Producir(_d1);
        await Transformar(TipoProducto.Queso, 100, _d1);
        await Transformar(TipoProducto.Yogurt, 50, _d1);
        await Producir(_d2);
        await Transformar(TipoProducto.Queso, 150, _d2);
        await Vender(TipoProducto.Queso, 12, _d3);
        await Vender(TipoProducto.Yogurt, 20, _d3);
        await _inventario.ProcesarVencimientosAsync(new VencimientoRequest(_d17));
        var informe = await _reportes.ProductoTotalAsync();
        var queso = informe.Single(x => x.Producto == TipoProducto.Queso);
        var yogurt = informe.Single(x => x.Producto == TipoProducto.Yogurt);
        Assert.Equal((25m, 12m, 13m, 0m), (queso.Producido, queso.Vendido, queso.Disponible, queso.Danado));
        Assert.Equal((50m, 20m, 0m, 30m), (yogurt.Producido, yogurt.Vendido, yogurt.Disponible, yogurt.Danado));
        var leche = await _reportes.LecheDiariaAsync();
        Assert.Equal(2, leche.Count);
        Assert.Equal(150m, leche[0].LitrosProducidos);
        Assert.Equal(150m, leche[1].LitrosProducidos);
        Assert.Equal(0m, leche[0].LitrosNoAprovechados);
    }
}

// Prueba de integración específica para el error reportado al arrancar Granja.Web.
public sealed class MigracionInicialTests
{
    [Fact]
    public async Task LaMigracionInicialPermiteArrancarYSembrarLaGranja()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<GranjaDbContext>()
            .UseSqlite(connection)
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;
        await using var db = new GranjaDbContext(options);
        await DbSeeder.InicializarAsync(db);
        Assert.Contains("20261120000000_Inicial", await db.Database.GetAppliedMigrationsAsync());
        Assert.Equal(1, await db.Granjas.CountAsync());
        Assert.Equal(10, await db.Vacas.CountAsync());
        Assert.Equal(2, await db.ConfiguracionesProducto.CountAsync());
        // La inicialización debe poder repetirse sin duplicar filas.
        await DbSeeder.InicializarAsync(db);
        Assert.Equal(10, await db.Vacas.CountAsync());
    }
}
