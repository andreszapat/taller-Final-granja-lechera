using AutoMapper;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Services.Dtos;
using Services.Implementations;
using Services.Mapping;

namespace Granja.Tests
{
    /// <summary>Reloj controlable para simular varios días (RN-10).</summary>
    public class FakeClock : IClock
    {
        public DateOnly Today { get; set; } = new(2026, 12, 1);
    }

    /// <summary>
    /// Arma los servicios reales sobre una base SQLite en memoria con los datos semilla
    /// (n = 10, l = 15, queso 10 L/kg 30 días, yogurt 1 L/L 15 días).
    /// </summary>
    public sealed class TestGranja : IDisposable
    {
        public static readonly DateOnly Dia1 = new(2026, 12, 1);
        public static DateOnly Dia(int n) => Dia1.AddDays(n - 1);

        private readonly SqliteConnection _connection;
        public GranjaDbContext Db { get; }
        public FakeClock Clock { get; } = new();
        public IMapper Mapper { get; }

        public GranjaService Granja { get; }
        public ProduccionService Produccion { get; }
        public InventarioService Inventario { get; }
        public VentaService Ventas { get; }
        public ReporteService Reportes { get; }

        public TestGranja()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            Db = NuevoContexto();
            Db.Database.Migrate();

            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<GranjaProfile>(), NullLoggerFactory.Instance).CreateMapper();

            var granjas = new GranjaRepository(Db);
            var vacas = new VacaRepository(Db);
            var producciones = new ProduccionLecheRepository(Db);
            var lotes = new LoteRepository(Db);
            var ventas = new VentaRepository(Db);
            var uow = new UnitOfWork(Db);

            Granja = new GranjaService(granjas, vacas, uow, Mapper);
            Produccion = new ProduccionService(granjas, vacas, producciones, lotes, uow, Clock, Mapper);
            Inventario = new InventarioService(lotes, granjas, uow, Clock, Mapper);
            Ventas = new VentaService(lotes, ventas, granjas, uow, Clock, Mapper);
            Reportes = new ReporteService(producciones, lotes, ventas, granjas, Mapper);
        }

        /// <summary>Contexto nuevo sobre la misma base, para verificar lo que quedó guardado.</summary>
        public GranjaDbContext NuevoContexto() =>
            new(new DbContextOptionsBuilder<GranjaDbContext>().UseSqlite(_connection).Options);

        public Task<ProduccionDiaResponse> ProducirAsync(DateOnly fecha) =>
            Produccion.RegistrarDiaAsync(new RegistrarProduccionRequest { Fecha = fecha });

        public Task<LoteResponse> TransformarAsync(DateOnly fecha, TipoProducto tipo, decimal litros) =>
            Produccion.TransformarAsync(new TransformacionRequest { Fecha = fecha, TipoProducto = tipo, Litros = litros });

        public Task<VentaResponse> VenderAsync(DateOnly fecha, TipoProducto tipo, decimal cantidad) =>
            Ventas.RegistrarVentaAsync(new VentaRequest { Fecha = fecha, TipoProducto = tipo, Cantidad = cantidad });

        public void Dispose()
        {
            Db.Dispose();
            _connection.Dispose();
        }
    }
}
