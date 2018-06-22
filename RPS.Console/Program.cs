using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace RPS.Console
{
    class Program
    {
        private static HubConnection _connection;
        private static string _gameId;

        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello World!");

            StartUp().Wait();

            while (true)
            {
                var message = System.Console.ReadLine();
                if (_gameId != null && int.TryParse(message, out var move))
                {
                    Fight(_gameId, move);
                }
                else
                {
                    Send(message);
                }
                
            }
        }

        private static async Task StartUp()
        {
            System.Console.WriteLine("Starting up");

            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl(Environment.GetEnvironmentVariable("HUB_URL"))
                    .Build();

                _connection.On<string>("ReceiveMessage", (message) =>
                {
                    System.Console.ForegroundColor = ConsoleColor.Cyan;
                    System.Console.WriteLine(message);
                    System.Console.ResetColor();
                });

                _connection.On<string>("StartGame", (gameId) =>
                {
                    _gameId = gameId;
                    System.Console.ForegroundColor = ConsoleColor.DarkRed;
                    System.Console.WriteLine($"Started game with id: {_gameId}");
                    System.Console.ResetColor();
                });

                await _connection.StartAsync();

                System.Console.WriteLine("Started!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }

        private static async void Send(string message)
        {
            try
            {
                await _connection.InvokeAsync("SendMessage", message);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }

        private static async void Fight(string gameId, int move)
        {
            try
            {
                await _connection.InvokeAsync("Fight", gameId, move);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }
}
