using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RPS.Domain;
using RPS.Domain.Enums;
using RPS.Pages;
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

            CanFight = true;

            CmdRock = new Command(() => Task.Factory.StartNew(Rock));
            CmdPaper = new Command(() => Task.Factory.StartNew(Paper));
            CmdScissors = new Command(() => Task.Factory.StartNew(Scissors));
            CmdQuit = new Command(() => Task.Factory.StartNew(Quit));

            _gameService.GameResult += OnGameResult;
            _gameService.GameQuit += OnOpponentQuit;
        }

        private void OnGameResult(GameResult result)
        {
            switch (result)
            {
                case GameResult.Win:
                    GameStatus = "You won!";
                    break;
                case GameResult.Draw:
                    GameStatus = "Draw";
                    break;
                case GameResult.Lose:
                    GameStatus = "You lost.";
                    break;
            }

            CanFight = true;
        }

        public ICommand CmdRock { get; set; }
        public ICommand CmdPaper { get; set; }
        public ICommand CmdScissors { get; set; }
        public ICommand CmdQuit { get; set; }

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

        private bool _canFight;
        public bool CanFight
        {
            get => _canFight;
            set
            {
                _canFight = value;
                OnPropertyChanged();
            }
        }

        private async Task Rock()
        {
            await Fight(GameMove.Rock);
        }

        private async Task Paper()
        {
            await Fight(GameMove.Paper);
        }

        private async Task Scissors()
        {
            await Fight(GameMove.Scissors);
        }

        private async Task Fight(GameMove move)
        {
            CanFight = false;
            GameStatus = $"You chose {move}. Waiting on opponent...";
            await _gameService.Fight(GameId, move);
        }

        private async Task Quit()
        {
            _gameService.Quit();

            if (Application.Current.MainPage is NavigationPage navPage)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await navPage.PopAsync();
                });
            }
        }

        private void OnOpponentQuit(string message)
        {
            CanFight = false;
            GameStatus = "Opponent has quit.";
        }
    }
}
