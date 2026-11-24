using Microsoft.Extensions.DependencyInjection;
using Services.Implementations;
using Services.Interfaces;
using Services.Mapping;

namespace Services
{
    public static class DependencyInjection
    {
        /// <summary>Registra los casos de uso y la configuración de AutoMapper.</summary>
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => cfg.AddProfile<GranjaProfile>());

            services.AddScoped<IGranjaService, GranjaService>();
            services.AddScoped<IVacaService, VacaService>();
            services.AddScoped<IProduccionService, ProduccionService>();
            services.AddScoped<IInventarioService, InventarioService>();
            services.AddScoped<IVentaService, VentaService>();
            services.AddScoped<IReporteService, ReporteService>();

            return services;
        }
    }
}
