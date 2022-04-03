using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotColour.Data;
using HotColour.Data.Game;
using HotColour.Data.GameLobby;
using HotColour.Data.Response;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HotColour.Services
{
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
                return DataResponse<HostResponse>.Fail("Failed to create session");
            }

            return DataResponse<HostResponse>.Success(new HostResponse(sessionId));
        }

        public async Task<DataResponse<JoinResponse>> JoinGame(string sessionId, NewPlayer newPlayer, CancellationToken cancellationToken = default)
        {
            var session = _gameLobby.GetSession(sessionId);
            if (session == null)
            {
                return DataResponse<JoinResponse>.Fail("Failed to load session");
            }
            
            // TODO: Don't allow more than n players (maybe with a dictionary?)
            if (!session.AddPlayer(newPlayer, out string playerId))
            {
                return DataResponse<JoinResponse>.Fail("Failed to add player to session");
            }

            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId, cancellationToken);
                await Clients.Group(sessionId).SendAsync(SessionStates.JoinedGame, cancellationToken);

                return DataResponse<JoinResponse>.Success(new JoinResponse(playerId));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error when {Player} tried to join game {Session}", newPlayer, sessionId);
                session.RemovePlayer(playerId);
                return DataResponse<JoinResponse>.Fail("Unexpected error");
            }
        }
        
        public List<Player> GetPlayers(string sessionId)
        {
            var session = _gameLobby.GetSession(sessionId);
            return session != null
                ? session.GetPlayers()
                : new List<Player>();
        }
    }
}