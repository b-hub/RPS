using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using RPS.Domain;
using RPS.Domain.Enums;
using RPS.Services.Interfaces;

namespace RPS.Services
{
    public class GameHubProxy : IGameService
    {
        private static HubConnection _connection;

        public async Task FindGame()
        {
            try
            {
                if (_connection != null)
                    await _connection.DisposeAsync();

                _connection = new HubConnectionBuilder()
                    .WithUrl("http://rps-server.azurewebsites.net/gamehub")
                    .Build();

                _connection.On<string>("ReceiveMessage", (message) =>
                {
                    Console.WriteLine(message);
                });

                _connection.On<string>("StartGame", (gameId) =>
                {
                    Console.WriteLine(gameId);
                    GameFound?.Invoke(gameId);
                });

                _connection.On<GameResult>("GameResult", (result) =>
                {
                    GameResult?.Invoke(result);
                });

                _connection.On<string>("QuitGame", async (message) =>
                {
                    GameQuit?.Invoke(message);
                });

                await _connection.StartAsync();

                Console.WriteLine("Started!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task Fight(string gameId, GameMove move)
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

        public void Quit()
        {
            Task.Factory.StartNew(async () => { await _connection.DisposeAsync(); });
        }

        public event OnGameFound GameFound;
        public event OnGameResult GameResult;
        public event OnGameQuit GameQuit;
    }
}
