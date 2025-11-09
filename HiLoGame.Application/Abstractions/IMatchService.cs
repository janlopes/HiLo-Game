using HiLoGame.Infrastructure.Data;
using HiLoGame.Infrastructure.Persistence;
using HiLoGame.Application.Models;
namespace HiLoGame.Application.Abstractions
{
    public interface IMatchService
    {
        Task PersistFinishedMatchAsync(GameRoom room, CancellationToken ct =
        default);
    }
}