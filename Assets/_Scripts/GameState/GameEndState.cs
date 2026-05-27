using PurrNet;
using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEndState : StateNode
{
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!InstanceHandler.TryGetInstance(out ScoreManager scoreManager))
        {
            //var winner = scoreManager.GetWinner();
            //Debug.Log($"The winner is: {winner}");
            Debug.Log("No ScoreManager instance found, cannot determine winner", this);
        }

        var winner = scoreManager.GetWinner();

        if (winner == default)
        {
            Debug.Log("No winner could be determined", this);
            return;
        }
        

            Debug.Log($"Game has now ended witch {winner} as our winner chappio");

    }
}
