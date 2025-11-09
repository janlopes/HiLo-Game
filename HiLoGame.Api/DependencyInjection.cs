using HiLoGame.Infrastructure.Data;
using HiLoGame.Infrastructure.Persistence;
using HiLoGame.Infrastructure.State;
using Microsoft.EntityFrameworkCore;

namespace HiLoGame.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this
        IServiceCollection services, string? connString)
        {
            services.AddDbContext<GameDbContext>(opt =>
            opt.UseSqlite(connString ?? "Data Source=hilo.db"));
            services.AddScoped<IMatchRepository, MatchRepository>();
            services.AddDistributedMemoryCache(); // swap to Redis easily
            services.AddSingleton<Application.Abstractions.IStateStore, DistributedCacheStateStore>();
            return services;
        }
    }
}
