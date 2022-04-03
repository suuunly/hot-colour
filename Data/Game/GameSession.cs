using System;
using System.Collections.Generic;
using System.Linq;

namespace HotColour.Data.Game
{
    public class GameSession
    {
        private readonly Dictionary<string, Player> _players;
        
        public GameSession()
        {
            _players = new Dictionary<string, Player>();
        }

        public bool AddPlayer(NewPlayer newPlayer, out string playerId)
        {
            playerId = Guid.NewGuid().ToString();
            _players.TryAdd(playerId, new Player(playerId, newPlayer.Name, newPlayer.Avatar));

            return true;
        }

        public void RemovePlayer(string playerId)
        {
            _players.Remove(playerId);
        }

        public List<Player> GetPlayers()
        {
            return _players.Select(p => p.Value).ToList();
        }
    }
}