using HotColour.Data;
using HotColour.Data.Game;
using HotColour.Data.GameLobby;
using HotColour.Data.Response;
using Microsoft.AspNetCore.SignalR;

namespace HotColour.Services;

public class SessionHub : Hub
{
    private readonly GameLobby _gameLobby;

    private readonly ILogger<SessionHub> _logger;

    public SessionHub(ILogger<SessionHub> logger)
    {
        _logger = logger;
        _gameLobby = new GameLobby();
    }

    public DataResponse<HostResponse> HostGame()
    {
        var sessionId = _gameLobby.CreateSession();
        if (sessionId == string.Empty)
        {
            return DataResponse<HostResponse>.Fail(TypeOfFailure.CouldNotCreateGame);
        }

        var session = _gameLobby.GetSession(sessionId);
        session.SetRoundCallbacks(
            async () => { await Clients.Group(sessionId).SendAsync(SessionCallbacks.RoundEnded); },
            async () => { await Clients.Group(sessionId).SendAsync(SessionCallbacks.GameEnded); }
        );

        return DataResponse<HostResponse>.Success(new HostResponse(sessionId));
    }

    public async Task<DataResponse<JoinResponse>> JoinGame(string sessionId, NewPlayer newPlayer,
        CancellationToken cancellationToken = default)
    {
        if (!_gameLobby.TryGetSession(sessionId, out var session))
        {
            return DataResponse<JoinResponse>.Fail(TypeOfFailure.CouldNotFindGame);
        }

        // TODO: Don't allow more than n players (maybe with a dictionary?)
        if (!session.AddPlayer(newPlayer, out var playerId))
        {
            return DataResponse<JoinResponse>.Fail(TypeOfFailure.CouldNotJoinGame);
        }

        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId, cancellationToken);
            await Clients.Group(sessionId).SendAsync(SessionCallbacks.JoinedGame, cancellationToken);

            return DataResponse<JoinResponse>.Success(new JoinResponse(playerId));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error when {Player} tried to join game {Session}", newPlayer, sessionId);
            session.RemovePlayer(playerId);
            return DataResponse<JoinResponse>.Fail(TypeOfFailure.Unexpected);
        }
    }

    public async Task<DataResponse> StartGame(string sessionId, CancellationToken cancellationToken = default)
    {
        if (!_gameLobby.TryGetSession(sessionId, out var session))
        {
            return DataResponse.Fail(TypeOfFailure.CouldNotFindGame);
        }

        var startedResult = session.StartGame();

        if (startedResult)
        {
            session.GenerateNewColourTarget();
            await Clients.Group(sessionId).SendAsync(SessionCallbacks.StartedGame, cancellationToken);
        }

        return startedResult
            ? DataResponse.Success()
            : DataResponse.Fail(TypeOfFailure.NotEnoughPlayers);
    }

    public List<Player> GetPlayers(string sessionId)
    {
        return _gameLobby.TryGetSession(sessionId, out var session)
            ? session.GetPlayers()
            : new List<Player>();
    }

    public DataResponse<GameSessionData> GameSessionData(string sessionId)
    {
        return _gameLobby.TryGetSession(sessionId, out var session)
            ? DataResponse<GameSessionData>.Success(session.GetGameData())
            : DataResponse<GameSessionData>.Fail(TypeOfFailure.CouldNotFindGame);
    }

    public async Task<DataResponse> GuessColour(string sessionId, HueColour colour, string playerId,
        CancellationToken cancellationToken = default)
    {
        if (!_gameLobby.TryGetSession(sessionId, out var session))
        {
            return DataResponse.Fail(TypeOfFailure.CouldNotFindGame);
        }

        if (!session.GuessColour(playerId, colour))
        {
            return DataResponse.Fail(TypeOfFailure.GameNotStarted);
        }

        await Clients.Group(sessionId).SendAsync(SessionCallbacks.GuessedColour, cancellationToken);

        return DataResponse.Success();
    }
}