using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        /// <summary>Registra el DbContext, los repositorios, la unidad de trabajo y el reloj.</summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connection = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=granja.db";
            var provider = configuration["DatabaseProvider"] ?? "Sqlite";

            services.AddDbContext<GranjaDbContext>(options =>
            {
                if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                    options.UseSqlServer(connection);
                else
                    options.UseSqlite(connection);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IGranjaRepository, GranjaRepository>();
            services.AddScoped<IVacaRepository, VacaRepository>();
            services.AddScoped<IProduccionLecheRepository, ProduccionLecheRepository>();
            services.AddScoped<ILoteRepository, LoteRepository>();
            services.AddScoped<IVentaRepository, VentaRepository>();
            services.AddSingleton<IClock, SystemClock>();

            return services;
        }
    }
}
