using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using RPS.Domain;

/* SET HUB_URL= */

namespace RPS.Console
{
    class Program
    {
        private static HubConnection _connection;
        private static string _gameId;
        private static bool hasQuit;

        static void Main(string[] args)
        {
            StartUp().Wait();

            while (!hasQuit)
            {
                var message = System.Console.ReadLine();
 
                if (_gameId != null && int.TryParse(message, out var move))
                {
                    Fight(_gameId, move).Wait();
                    continue;
                }

                if (message == "q" || message == "quit")
                {
                    Quit(_gameId).Wait();
                }
                else
                {
                    Send(message).Wait();
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
                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.WriteLine($"Started game with id: {_gameId}");
                    System.Console.ResetColor();
                });

                _connection.On<GameResult>("GameResult", (result) =>
                {
                    System.Console.ForegroundColor = ConsoleColor.Blue;
                    System.Console.WriteLine(result.ToString());
                    System.Console.ResetColor();
                });

                _connection.On<string>("QuitGame", async (message) =>
                {
                    _gameId = null;
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine(message);
                    System.Console.ResetColor();
                    OnQuit();
                });

                await _connection.StartAsync();

                System.Console.WriteLine("Started!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }

        private static async Task Send(string message)
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

        private static async Task Fight(string gameId, int move)
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

        private static async Task Quit(string gameId)
        {
            try
            {
                var result = await _connection.InvokeAsync<(bool success, string message)>("Quit", gameId);
                System.Console.WriteLine(result.message);

                if (result.success)
                {
                    OnQuit();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }

        private static void OnQuit()
        {
            _connection?.StopAsync();
            _connection?.DisposeAsync();

            Environment.Exit(0);
        }
    }
}
