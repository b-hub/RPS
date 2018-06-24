using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RPS.Domain.Enums;

namespace RPS.Services.Interfaces
{
    public interface IGameService
    {
        Task FindGame();
        Task Fight(string gameId, GameMove move);

        event OnGameFound GameFound;
        event OnGameResult GameResult;
    }

    public delegate void OnGameFound(string gameId);
    public delegate void OnGameResult(string gameId);
}
