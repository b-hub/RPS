using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RPS.Domain.Enums;
using RPS.Services;
using RPS.Services.Interfaces;
using Xamarin.Forms;

namespace RPS.ViewModels
{
    public class GamePageVM : Observable
    {
        private readonly IGameService _gameService;

        public GamePageVM(IGameService gameService)
        {
            _gameService = gameService;

            CmdRock = new Command(async () => await Fight(GameMove.Rock));
            CmdPaper = new Command(async () => await Fight(GameMove.Paper));
            CmdScissors = new Command(async () => await Fight(GameMove.Scissors));

            _gameService.GameResult += OnGameResult;
        }

        private void OnGameResult(string gameid)
        {
            throw new NotImplementedException();
        }

        public ICommand CmdRock { get; set; }
        public ICommand CmdPaper { get; set; }
        public ICommand CmdScissors { get; set; }

        public string GameId { get; set; }

        private string _gameStatus;
        public string GameStatus
        {
            get => _gameStatus;
            set
            {
                _gameStatus = value;
                OnPropertyChanged();
            }
        }

        private async Task Fight(GameMove move)
        {
            GameStatus = $"You chose {move}. Waiting on opponent...";
            await _gameService.Fight(GameId, move);
        }
    }
}
