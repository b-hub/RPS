using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using RPS.Pages;
using RPS.Services;
using RPS.Services.Interfaces;
using RPS.ViewModels;
using Xamarin.Forms;

namespace RPS
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
            InitializeComponent();
		    NavigationPage.SetHasNavigationBar(this, false);

            var gameService = new GameHubProxy();
		    gameService.GameFound += async gameId => await OnGameFound(gameId);
            BindingContext = new MainPageVM(gameService);
		}

	    private async Task OnGameFound(string gameid)
	    {
	        var gamePageVM = new GamePageVM
	        {
                GameId = gameid
	        };

	        var page = new GamePage {BindingContext = gamePageVM};
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
