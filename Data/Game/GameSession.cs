using HotColour.Common.Extensions;

namespace HotColour.Data.Game;

public class GameSession
{
    private static readonly TimeSpan RoundDuration = new(0, 0, 0, 30);

    private readonly Dictionary<string, Player> _players;
    private readonly Random _rnd;

    private readonly CancellationTokenSource _roundEndCancellationTokenSource;
    private GameState _currentGameState;
    private int _currentPlayerTurnIndex;

    private List<string> _playerOrder;
    private HueColour _targetColour;


    public GameSession()
    {
        _currentPlayerTurnIndex = 0;
        _players = new Dictionary<string, Player>();

        _currentGameState = GameState.WaitingForPlayers;

        _targetColour = new HueColour(0, 0, 0);
        _rnd = new Random();

        _roundEndCancellationTokenSource = new CancellationTokenSource();

        GenerateNewColourTarget();
    }

    private DateTime RoundTerminationTime { get; set; }
    private DateTime RoundStartedTime { get; set; }

    private string PlayerIdOfCurrentPlayer => _playerOrder[_currentPlayerTurnIndex];

    public bool AddPlayer(NewPlayer newPlayer, out string playerId)
    {
        playerId = Guid.NewGuid().ToString();

        var (name, avatar) = newPlayer;

        _players.TryAdd(playerId, new Player(playerId, name, avatar));
        _playerOrder = _players.Select(p => p.Value.Id).ToList();

        return true;
    }

    public void RemovePlayer(string playerId)
    {
        if (!_players.Remove(playerId))
        {
            return;
        }

        _playerOrder = _players.Select(p => p.Value.Id).ToList();
    }

    public List<Player> GetPlayers()
    {
        return _players.Select(p => p.Value).ToList();
    }

    private void NextPlayer()
    {
        // TODO: Skip dead players (maybe remove them from the list?)
        _currentPlayerTurnIndex++;
        if (_currentPlayerTurnIndex > _players.Count - 1)
        {
            _currentPlayerTurnIndex = 0;
        }
    }

    public bool StartGame(Func<Task> onRoundEnded, Func<Task> onGameEnded)
    {
        if (_players.Count < 2)
        {
            return false;
        }

        _playerOrder.Shuffle();
        _currentPlayerTurnIndex = 0;

        _currentGameState = GameState.GameIsActive;


        // TODO: Reset Delay After Move

        // TODO: Reset Delay after Round ended
        StartNewRound(onRoundEnded, onGameEnded);

        return true;
    }

    private void StartNewRound(Func<Task> roundEndedCallback, Func<Task> gameEndedCallback)
    {
        RoundStartedTime = DateTime.Now;
        RoundTerminationTime = DateTime.Now + RoundDuration;
        var totalTime = (RoundTerminationTime - RoundStartedTime).TotalSeconds;


        var cancellationToken = _roundEndCancellationTokenSource.Token;
        Task.Delay(TimeSpan.FromSeconds(totalTime), cancellationToken).ContinueWith(t =>
        {
            if (!_players.TryGetValue(PlayerIdOfCurrentPlayer, out var player))
            {
                return;
            }

            var playerDied = player with {IsDead = true};
            _players[PlayerIdOfCurrentPlayer] = playerDied;

            NextPlayer();

            roundEndedCallback();

            if (_players.Count(p => p.Value.IsDead) >= 2)
            {
                StartNewRound(roundEndedCallback, gameEndedCallback);
                return;
            }

            _currentGameState = GameState.GameEnded;
            gameEndedCallback();
        }, cancellationToken);
    }

    public void GenerateNewColourTarget()
    {
        var h = _rnd.Next(1, 360);
        // var s = _rnd.Next(0, 100);
        var l = _rnd.Next(50, 100);
        _targetColour = new HueColour(h, 100, l);
    }

    public GameSessionData GetGameData()
    {
        return new GameSessionData(
            GetPlayers(),
            _currentGameState,
            PlayerIdOfCurrentPlayer,
            _targetColour,
            RoundStartedTime,
            RoundTerminationTime
        );
    }

    public bool GuessColour(HueColour colour)
    {
        if (_currentGameState != GameState.GameIsActive)
        {
            return false;
        }

        var (h1, s1, l1) = _targetColour;
        var (h2, s2, l2) = colour;

        var h = h1 > h2 ? h1 - h2 : h2 - h1;
        var s = s1 > s2 ? s1 - s2 : s2 - s1;
        var l = l1 > l2 ? l1 - l2 : l2 - l1;

        // https://en.wikipedia.org/wiki/Color_difference
        var distance = Math.Sqrt(Math.Pow(h, 2) + (Math.Pow(s, 2) + Math.Pow(l, 2)));
        var percentage = distance / Math.Sqrt(Math.Pow(360, 2) + Math.Pow(100, 2) + Math.Pow(100, 2));

        var accuracy = (1.0f - percentage) * 100.0f;

        // todo: use the accuracy
        RoundTerminationTime = RoundTerminationTime.Add(new TimeSpan(0, 0, 20));

        GenerateNewColourTarget();

        NextPlayer();

        return true;
    }
}