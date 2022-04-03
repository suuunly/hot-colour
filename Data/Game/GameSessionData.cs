using System;
using System.Collections.Generic;

namespace HotColour.Data.Game
{
    public record GameSessionData(
        List<Player> Players, 
        GameState GameState, 
        string PlayerIdsTurn, 
        HueColour TargetColour,
        DateTime TimeWhenRoundStarted,
        DateTime TimeWhenRoundEnds
    );
}