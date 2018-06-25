using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RPS.Domain;
using RPS.Domain.Enums;

namespace RPS.Services.Interfaces
{
    public interface IGameService
    {
        Task FindGame();
        Task Fight(string gameId, GameMove move);
        void Quit();

        event OnGameFound GameFound;
        event OnGameResult GameResult;
        event OnGameQuit GameQuit;
    }

    public delegate void OnGameFound(string gameId);
    public delegate void OnGameResult(GameResult result);
    public delegate void OnGameQuit(string message);
}
