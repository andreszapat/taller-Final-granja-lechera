using System.Text.Json.Serialization;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Granja.Infrastructure.Repositories;
using Granja.Services.Mapping;
using Granja.Services.Services;
using Granja.Web.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    { Title = "API Granja Lechera", Version = "v1", Description = "Taller Lenguajes de Programación - USC" });
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GranjaExceptionHandler>();
builder.Services.AddDbContext<GranjaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Granja") ?? "Data Source=granja.db"));
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IGranjaRepository, GranjaRepository>();
builder.Services.AddScoped<IVacaRepository, VacaRepository>();
builder.Services.AddScoped<IConfiguracionProductoRepository, ConfiguracionProductoRepository>();
builder.Services.AddScoped<IProduccionRepository, ProduccionRepository>();
builder.Services.AddScoped<ITransformacionRepository, TransformacionRepository>();
builder.Services.AddScoped<ILoteRepository, LoteRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<GranjaService>();
builder.Services.AddScoped<VacaService>();
builder.Services.AddScoped<ProduccionService>();
builder.Services.AddScoped<InventarioService>();
builder.Services.AddScoped<VentaService>();
builder.Services.AddScoped<ReporteService>();

var app = builder.Build();
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.InicializarAsync(scope.ServiceProvider.GetRequiredService<GranjaDbContext>());
}
app.Run();
public partial class Program { }
