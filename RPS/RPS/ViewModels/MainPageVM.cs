using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Android.Content.Res;
using RPS.Pages;
using RPS.Services;
using RPS.Services.Interfaces;
using Xamarin.Forms;

namespace RPS.ViewModels
{
    public class MainPageVM : Observable
    {
        private bool _findingGame = false;
        private IGameService _gameService;

        public MainPageVM(IGameService gameService)
        {
            _gameService = gameService;

            ButtonText = "Find a game";
            CmdFindGame = new Command(() => Task.Factory.StartNew(FindGame));

            gameService.GameFound += OnGameFound;
        }

        public ICommand CmdFindGame { get; set; }

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

        private string _buttonText;
        public string ButtonText
        {
            get => _buttonText;
            set
            {
                _buttonText = value;
                OnPropertyChanged();
            }
        }

        private async Task FindGame()
        {
            if (_findingGame)
                return;

            _findingGame = true;
            ButtonText = "Cancel";
            GameStatus = "Connecting...";
            await _gameService.FindGame();
            GameStatus = "Finding a game...";
        }

        private void OnGameFound(string gameid)
        {
            var gamePageVM = new GamePageVM(_gameService)
            {
                GameId = gameid
            };

            var page = new GamePage { BindingContext = gamePageVM };
            if (Application.Current.MainPage is NavigationPage navPage)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await navPage.PushAsync(page);
                });
            }
        }
    }
}
