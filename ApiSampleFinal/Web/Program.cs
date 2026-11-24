using System.Reflection;
using System.Text.Json.Serialization;
using ApiSampleFinal.Middleware;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Services;

namespace ApiSampleFinal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Raíz de composición: Web conoce Infrastructure solo para registrar dependencias.
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddServices();

            builder.Services.AddControllers()
                .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            // Manejo global de errores con ProblemDetails (400, 404, 409, 500).
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Granja Lechera API",
                    Version = "v1",
                    Description = "Producción de leche, transformación en queso/yogurt, inventario FIFO por lotes, ventas y reportes."
                });
                c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date", Example = new Microsoft.OpenApi.Any.OpenApiString("2026-12-01") });
                var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                if (File.Exists(xml)) c.IncludeXmlComments(xml);
            });

            builder.Services.AddCors(p => p.AddPolicy("CORS_Policy", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<GranjaDbContext>();
                db.Database.Migrate();
            }

            app.UseExceptionHandler();
            app.UseStatusCodePages();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.DocumentTitle = "Granja Lechera API");

            app.UseCors("CORS_Policy");
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

            app.Run();
        }
    }
}
