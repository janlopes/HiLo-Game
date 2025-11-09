using HiLoGame.Application.Abstractions;
using HiLoGame.Infrastructure.Data;
using HiLoGame.Infrastructure.Persistence;
using HiLoGame.Infrastructure.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class InfraDependencyInjection
{
    public static IServiceCollection AddHiLoInfrastructure(
        this IServiceCollection services, string? connString)
    {
        services.AddDbContext<GameDbContext>(opt =>
            opt.UseSqlite(connString));

        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddDistributedMemoryCache();
        services.AddSingleton<IStateStore, DistributedCacheStateStore>();
        return services;
    }
}