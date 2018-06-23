using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RPS.Services.Interfaces
{
    public interface IGameService
    {
        Task FindGame();
        event OnGameFound GameFound;
    }

    public delegate void OnGameFound(string gameId);
}
