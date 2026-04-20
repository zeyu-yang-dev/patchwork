using System;
using System.Collections.Generic;

namespace Patchwork.Domain;

/// <summary>
/// Represents the state of a Patchwork game.
/// </summary>
public class GameState
{
    public List<Player> Players { get; }

    public int CurrentPlayerIndex { get; set; }
    public Player CurrentPlayer => Players[CurrentPlayerIndex];

    public PlacedPatch CurrentPlacedPatch { get; set; }
    
    public PatchShop PatchShop { get; }
    public Timeline Timeline { get; }

    // -1 means no player has reached the end position first yet.
    public int FirstPlayerIndexToReachEnd { get; set; } = -1;

    // =================================================================================================================

    public GameState(string firstPlayerName, string secondPlayerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstPlayerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(secondPlayerName);

        Players =
        [
            new Player(firstPlayerName),
            new Player(secondPlayerName)
        ];
        CurrentPlayerIndex = 0;
        PatchShop = new PatchShop();
        Timeline = new Timeline();
    }
    
    // =================================================================================================================

    public bool IsGameOver()
    {
        foreach (var player in Players)
        {
            if (player.TimePosition < Timeline.EndPosition)
            {
                return false;
            }
        }

        return true;
    }

    public Player GetWinner()
    {
        if (!IsGameOver())
        {
            return null;
        }

        if (Players[0].Score == Players[1].Score)
        {
            
            return Players[FirstPlayerIndexToReachEnd];
        }

        return Players[0].Score > Players[1].Score ? Players[0] : Players[1];
    }
}
