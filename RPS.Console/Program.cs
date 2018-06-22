using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace RPS.Console
{
    class Program
    {
        private static HubConnection connection;

        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello World!");

            StartUp().Wait();

            while (true)
            {
                var message = System.Console.ReadLine();
                Send(message);
            }
        }

        private static async Task StartUp()
        {
            System.Console.WriteLine("Starting up");

            try
            {
                connection = new HubConnectionBuilder()
                    .WithUrl(Environment.GetEnvironmentVariable("HUB_URL"))
                    .Build();

                connection.On<string>("ReceiveMessage", (message) =>
                {
                    System.Console.ForegroundColor = ConsoleColor.Cyan;
                    System.Console.WriteLine(message);
                    System.Console.ResetColor();
                });

                await connection.StartAsync();

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
                await connection.InvokeAsync("SendMessage", message);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
            
        }
    }
}
