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
            Game.OnResult += async (game, getResultFor) =>
            {
                var clients = globalContext.Clients;
                await clients.Client(game.Id1).SendAsync("GameResult", getResultFor(game.Id1));
                await clients.Client(game.Id2).SendAsync("GameResult", getResultFor(game.Id2));
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

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            _disconnected.TryAdd(connectionId, false);
            var games = _games.Where(x => x.Value.IsPlayer(connectionId));
            foreach (var game in games)
            {
                var opponentId = game.Value.OpponentId(connectionId);
                await Clients.Client(opponentId).SendAsync("QuitGame", $"{connectionId} has quit the game. Game summary: {game.Value.GameSummary(opponentId)}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task Fight(string gameId, int move)
        {
            if (!_games.TryGetValue(gameId, out var game))
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

        public async Task<(bool, string)> Quit(string gameId)
        {
            var connectionId = Context.ConnectionId;

            if (!_games.TryGetValue(gameId, out var game))
            {
                return (false, "Invalid game id");
            }

            if (!game.IsPlayer(connectionId))
            {
                return (false, "You are not a part of this game");
            }

            _games.TryRemove(gameId, out game);
            var opponentId = game.OpponentId(connectionId);

            await Clients.Client(opponentId).SendAsync("QuitGame", $"{connectionId} has quit the game. Game summary: {game.GameSummary(opponentId)}");
            return (true, game.GameSummary(connectionId));
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

        private IList<string> _results = new List<string>();
        private int GetWins(string playerId) => _results.Count(x => x == playerId);
        private int GetLosses(string playerId) => _results.Count(x => x == OpponentId(playerId));
        private int GetDraws() => _results.Count(x => x == null);

        public Game(string id, string id1, string id2)
        {
            Id = id;
            Id1 = id1;
            Id2 = id2;
        }

        public static event Result OnResult;

        public delegate void Result(Game sender, Func<string, GameResult> getResultFor);

        public string Id { get; }
        public string Id1 { get; }
        public string Id2 { get; }

        public bool IsPlayer(string playerId) => Id1 == playerId || Id2 == playerId;
        public string OpponentId(string playerId) => Id1 == playerId ? Id2 : Id1;

        public string GameSummary(string playerId)
        {
            return $"Wins: {GetWins(playerId)}, Losses: {GetLosses(playerId)}, Draws: {GetDraws()}";
        }
            
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
                var winningId = GetWinner();
                _move1 = null;
                _move2 = null;
                _results.Add(winningId);

                OnResult?.Invoke(this, playerId => GetResultFor(playerId, winningId));
            }
        }

        private GameResult GetResultFor(string playerId, string winningId)
        {
            if (winningId == null) return GameResult.Draw;

            return winningId == playerId ? GameResult.Win : GameResult.Lose;
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

    public enum GameResult
    {
        Win,
        Lose,
        Draw
    }
}
