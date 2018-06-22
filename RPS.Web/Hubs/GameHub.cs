using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace RPS.Web.Hubs
{
    public class GameHub : Hub
    {
        private static ConcurrentQueue<string> _waitingConnections = new ConcurrentQueue<string>();
        private static ConcurrentDictionary<string, bool> _disconnected = new ConcurrentDictionary<string, bool>();
        private static ConcurrentDictionary<string, Game> _games = new ConcurrentDictionary<string, Game>();

        private static bool IsListening = false;

        public GameHub(IHubContext<GameHub> globalContext)
        {
            if (IsListening) return;

            IsListening = true;
            Game.OnResult += async (game, winner) =>
            {
                var clients = globalContext.Clients;
                var message = $"Winner is: {winner}";
                await clients.Client(game.Id1).SendAsync("ReceiveMessage", message);
                await clients.Client(game.Id2).SendAsync("ReceiveMessage", message);
            };
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;

            if (_waitingConnections.IsEmpty)
            {
                _waitingConnections.Enqueue(Context.ConnectionId);
            }
            else if (_waitingConnections.TryDequeue(out var matchId))
            {
                while (_disconnected.ContainsKey(matchId))
                {
                    _disconnected.TryRemove(matchId, out var _);
                    _waitingConnections.TryDequeue(out matchId);
                }

                var gameId = Guid.NewGuid().ToString("N");
                var game = new Game(gameId, connectionId, matchId);
                
                _games.TryAdd(gameId, game);
                await StartGame(game);
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _disconnected.TryAdd(Context.ConnectionId, false);

            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task Fight(string gameId, int move)
        {
            _games.TryGetValue(gameId, out var game);
            if (game == null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "Invalid game id");
                return;
            }

            try
            {
                game.Fight(Context.ConnectionId, move);
            }
            catch (Exception ex)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", ex.Message);
            }
            
        }

        private async Task StartGame(Game game)
        {
            await Clients.All.SendAsync("ReceiveMessage", $"Game started between: {game.Id1} and {game.Id2}");
            await Clients.Client(game.Id1).SendAsync("StartGame", game.Id);
            await Clients.Client(game.Id2).SendAsync("StartGame", game.Id);
        }
    }

    public class Game
    {
        private static readonly int[][] MoveMapping = new []{new []{0, -1, 1}, new []{1, 0, -1}, new []{-1, 1, 0}};

        // change to make thread safe
        private int? _move1;
        private int? _move2;

        public Game(string id, string id1, string id2)
        {
            Id = id;
            Id1 = id1;
            Id2 = id2;
        }

        public static event Result OnResult;

        public delegate void Result(Game sender, string winner);

        public string Id { get; }
        public string Id1 { get; }
        public string Id2 { get; }

        public void Fight(string id, int move)
        {
            if (id == Id1)
            {
                if (_move1.HasValue) throw new Exception("Player has already chosen a move");
                _move1 = move;
            }
            else if (id == Id2)
            {
                if (_move2.HasValue) throw new Exception("Player has already chosen a move");
                _move2 = move;
            }
            else
            {
                throw new Exception($"Player {id} does not belong to this game");
            }

            if (_move1.HasValue && _move2.HasValue)
            {
                OnResult?.Invoke(this, GetWinner());
            }
        }

        private string GetWinner()
        {
            if (!_move1.HasValue || !_move2.HasValue)
                throw new Exception("Game must be complete before calling GetWinner");

            var res = MoveMapping[_move1.Value][_move2.Value];
            if (res == 1) return Id1;
            if (res == 0) return null;
            if (res == -1) return Id2;

            throw new Exception("Something went wrong.");
        }


    }
}
