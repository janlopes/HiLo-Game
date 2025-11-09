using HiLoGame.Application.Abstractions;
using HiLoGame.Application.Services;
using Microsoft.Extensions.DependencyInjection;

public static class GameDependecyInjection
{
    public static IServiceCollection AddGameDependecyInjection(
        this IServiceCollection services)
    {
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<IGameService, GameService>();
        return services;
    }
}