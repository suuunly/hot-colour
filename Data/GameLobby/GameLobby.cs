using System;
using System.Collections.Generic;
using HotColour.Data.Game;
using HotColour.Services;

namespace HotColour.Data.GameLobby
{
    public class GameLobby
    {
        private readonly Dictionary<string, GameSession> _sessions;

        public GameLobby()
        {
            _sessions = new Dictionary<string, GameSession>();
        }

        public string CreateSession()
        {
            var sessionId = Guid.NewGuid().ToString();
            return _sessions.TryAdd(sessionId, new GameSession()) 
                ? sessionId
                : string.Empty;
        }

        public bool DestroySession(string sessionId)
        {
            return _sessions.Remove(sessionId);
        }

        public GameSession GetSession(string sessionId)
        {
            return _sessions.TryGetValue(sessionId, out GameSession session) 
                ? session 
                : null;
        }
        
        public bool TryGetSession(string sessionId, out GameSession session)
        {
            session = GetSession(sessionId);
            return session == null;
        }
    }
}