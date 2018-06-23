using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
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
                });

                _connection.On<string>("QuitGame", async (message) =>
                {
                    Console.WriteLine(message);
                });

                await _connection.StartAsync();

                Console.WriteLine("Started!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
