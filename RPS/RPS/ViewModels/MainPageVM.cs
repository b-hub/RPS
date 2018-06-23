using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Android.Content.Res;
using RPS.Services;
using RPS.Services.Interfaces;
using Xamarin.Forms;

namespace RPS.ViewModels
{
    public class MainPageVM : INotifyPropertyChanged
    {
        private IGameService _gameService;

        public MainPageVM(IGameService gameService)
        {
            _gameService = gameService;

            CmdFindGame = new Command(async() => await FindGame());
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

        private async Task FindGame()
        {
            GameStatus = "Connecting...";
            await _gameService.FindGame();
            GameStatus = "Finding a game...";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
