using PurrNet;
using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEndState : StateNode<Dictionary<PlayerID, int>>
{
    public override void Enter(Dictionary<PlayerID, int> roundWins, bool asServer)
    {
        base.Enter(asServer);

        var winner = roundWins.First();

        foreach (var player in roundWins)
        {
            if (player.Value > winner.Value)
            {
                winner = player;
            }
        }

        Debug.Log($"Game has now ended with {winner} being our chapiio!!");

        roundWins.Clear();
    }
}
