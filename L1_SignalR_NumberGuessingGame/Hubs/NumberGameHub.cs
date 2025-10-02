using Microsoft.AspNetCore.SignalR;


namespace L1_SignalR_NumberGuessingGame.Hubs;

public class Player
{
    public static readonly int DefaultAttempts = 3;

    public string ConnectionId { get; set; }
    public string Name { get; set; }
    public DateTime JoinedAt { get; set; }
    public int AttemptsLeft { get; set; }

    public Player(string connectionId, string name)
    {
        ConnectionId = connectionId;
        Name = name;
        JoinedAt = DateTime.Now;
        AttemptsLeft = DefaultAttempts;
    }
}

public class GameInfo
{
    public int Number { get; set; }
    public Player GameOwner { get; set; }

    public GameInfo(Player player, int number)
    {
        Number = number;
        GameOwner = player;
    }
}

public static class EventName
{
    public const string JoinGame = "JoinGame";
    public const string PlayerJoined = "PlayerJoined";
    public const string PlayerDisconnected = "PlayerDisconnected";

    public const string MakeGuess = "MakeGuess";
    public const string OnGuess = "OnGuess";

    public const string NewGameStarted = "NewGameStarted";
    public const string GameEnded = "GameEnded";


    public const string ValidationFailure = "ValidationFailure";

    public const string OnPlayerListUpdated = "OnPlayerListUpdated";
    
    public const string GetOtherPlayers = "GetOtherPlayers";
}

public class NumberGameHub : Hub
{
    private static readonly List<Player> _players = [];
    private static GameInfo? _gameInfo = null;

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var player = _players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
        if (player != null)
        {
            _players.Remove(player);
            await Clients.All.SendAsync(EventName.PlayerDisconnected, player.Name, _players.Count);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGame(string name)
    {
        if (_players.Any(x => x.ConnectionId == Context.ConnectionId)) return;

        if (_players.Any(x => x.Name == name))
        {
            await Clients.Caller.SendAsync(EventName.ValidationFailure, "Player with this name is already in lobby");
            return;
        }

        var newPlayer = new Player(Context.ConnectionId, name);
        _players.Add(newPlayer);

        if (_gameInfo != null)
        {
            var message = $"Player {_gameInfo.GameOwner.Name} is thinking about some number...";
            await Clients.Caller.SendAsync(EventName.NewGameStarted, message);
        }

        await Clients.All.SendAsync(EventName.PlayerJoined, newPlayer.Name, _players.Count, newPlayer.AttemptsLeft);
    }

    public async Task MakeGuess(int number)
    {
        var player = _players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
        if (player == null) return;

        //Start game
        if (_gameInfo == null)
        {
            _gameInfo = new GameInfo(player, number);

            var message = $"Player {player.Name} is thinking about some number...";

            await Clients.All.SendAsync(EventName.NewGameStarted, message);
            _players.ForEach(p => p.AttemptsLeft = Player.DefaultAttempts);
            return;
        }

        //Restrict from self guessing
        if (_gameInfo.GameOwner == player)
        {
            await Clients.Caller.SendAsync(EventName.ValidationFailure, "Can`t guess as a game owner!");
            return;
        }

        if (player.AttemptsLeft == 0)
        {
            await Clients.Caller.SendAsync(EventName.ValidationFailure, "No attempts left!");
            return;
        }

        player.AttemptsLeft--;

        //Win
        if (_gameInfo.Number == number)
        {
            await Clients.All.SendAsync(EventName.GameEnded, $"{player.Name} won the game!");
            _gameInfo = null;
        }
        // Incorrect number
        else
        {
            var isTooHigh = _gameInfo.Number < number;
            await Clients.All.SendAsync(EventName.OnGuess, player.Name, number, isTooHigh, player.AttemptsLeft);
        }
    }

    public async Task GetOtherPlayers()
    {
        var players = _players.Where(p => p.ConnectionId != Context.ConnectionId).Select(x => (x.Name, x.AttemptsLeft));
        await Clients.Caller.SendAsync(EventName.GetOtherPlayers, players);
    }
}