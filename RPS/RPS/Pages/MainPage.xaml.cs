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
            BindingContext = new MainPageVM(gameService);
		}
	}
}
