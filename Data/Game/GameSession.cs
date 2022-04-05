using HotColour.Common.Extensions;

namespace HotColour.Data.Game;

public class GameSession
{
    private const float MinimumAccuracyRequired = 0.6f;
    private const float MaxTimeAdded = 3f;
    private static readonly TimeSpan RoundDuration = new(0, 0, 0, 10);

    private readonly List<Player> _players;

    private readonly Random _rnd;

    private readonly CancellationTokenSource _roundEndCancellationTokenSource;

    // private Timer _timer;
    private readonly RoundEndDelay _roundEndTimer;
    private GameState _currentGameState;
    private int _currentPlayerTurnIndex;
    private Func<Task> _gameEndedCallback;
    private HueColour _lastSelectedColour = new(0, 100, 100);
    private List<Player> _playerInstances;
    private Func<Task> _roundEndedCallback;

    private HueColour _targetColour;

    public GameSession()
    {
        _currentPlayerTurnIndex = 0;
        _players = new List<Player>();
        _playerInstances = new List<Player>();

        _currentGameState = GameState.WaitingForPlayers;

        _targetColour = new HueColour(0, 0, 0);
        _rnd = new Random();

        _roundEndCancellationTokenSource = new CancellationTokenSource();

        _roundEndTimer = new RoundEndDelay(RoundDuration, OnRoundEnded);

        GenerateNewColourTarget();
    }

    private string PlayerIdOfCurrentPlayer => _playerInstances[_currentPlayerTurnIndex].Id;

    public bool AddPlayer(NewPlayer newPlayer, out string playerId)
    {
        playerId = Guid.NewGuid().ToString();

        var (name, avatar) = newPlayer;
        _players.Add(new Player(playerId, name, avatar));

        _playerInstances = _players.ToList();

        return true;
    }

    public void RemovePlayer(string playerId)
    {
        _players.RemoveAll(p => p.Id == playerId);
        _playerInstances = _players.ToList();
    }

    public List<Player> GetPlayers()
    {
        return _playerInstances;
    }

    private void NextPlayer()
    {
        // TODO: Skip dead players (maybe remove them from the list?)
        _currentPlayerTurnIndex++;
        if (_currentPlayerTurnIndex > _players.Count - 1)
        {
            _currentPlayerTurnIndex = 0;
        }

        _roundEndCancellationTokenSource.Cancel();
    }

    public bool StartGame()
    {
        if (_players.Count < 2)
        {
            return false;
        }

        _playerInstances = _players.Select(player => player with {IsDead = false}).ToList();
        _playerInstances.Shuffle();
        _currentPlayerTurnIndex = 0;

        _currentGameState = GameState.GameIsActive;

        _roundEndTimer.Start();

        return true;
    }

    private void OnRoundEnded()
    {
        var currentPlayer = _playerInstances[_currentPlayerTurnIndex];

        var playerDied = currentPlayer with {IsDead = true};
        _playerInstances[_currentPlayerTurnIndex] = playerDied;

        NextPlayer();

        _roundEndedCallback();

        if (_playerInstances.Count(p => !p.IsDead) > 1)
        {
            _roundEndTimer.Start();
            return;
        }

        _currentGameState = GameState.GameEnded;
        _gameEndedCallback();
    }

    public void SetRoundCallbacks(Func<Task> roundEndedCallback, Func<Task> gameEndedCallback)
    {
        _roundEndedCallback = roundEndedCallback;
        _gameEndedCallback = gameEndedCallback;
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
            _roundEndTimer.StartTime,
            _roundEndTimer.RoundEndTime,
            _lastSelectedColour
        );
    }

    public bool GuessColour(string playerId, HueColour colour)
    {
        if (_currentGameState != GameState.GameIsActive)
        {
            return false;
        }

        if (PlayerIdOfCurrentPlayer != playerId)
        {
            return false;
        }

        _lastSelectedColour = colour;

        var (h1, s1, l1) = _targetColour;
        var (h2, s2, l2) = colour;

        var h = h1 > h2 ? h1 - h2 : h2 - h1;
        var s = s1 > s2 ? s1 - s2 : s2 - s1;
        var l = l1 > l2 ? l1 - l2 : l2 - l1;

        // https://en.wikipedia.org/wiki/Color_difference
        var distance = Math.Sqrt(Math.Pow(h, 2) + (Math.Pow(s, 2) + Math.Pow(l, 2)));
        var percentage = distance / Math.Sqrt(Math.Pow(360, 2) + Math.Pow(100, 2) + Math.Pow(100, 2));

        var accuracy = 1.0f - percentage;

        // if you're below 60%, then it is not considered.
        // Otherwise the max amount added is based on your accuracy
        var addedTime = accuracy > MinimumAccuracyRequired
            ? MaxTimeAdded * accuracy
            : 0;

        GenerateNewColourTarget();

        NextPlayer();

        _roundEndTimer.AddTime(TimeSpan.FromSeconds(addedTime));

        return true;
    }
}