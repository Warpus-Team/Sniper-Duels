using PurrNet;
using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundEndState : StateNode<PlayerID>
{
    [SerializeField] private int amountOfRounds = 3;
    [SerializeField] private StateNode spawningState;

    private int _roundCount = 0;
    private WaitForSeconds _daley = new(3f);

    private Dictionary<PlayerID, int> _roundWins = new();

    override public void Enter(bool asServer)
    {
        base.Enter(asServer);
        if (!asServer)
            return;
       
        Debug.Log("Round Ended with no winner");

        CheckForGameEnd();
    }

    public override void Enter(PlayerID winner, bool asServer)
    {
        base.Enter(asServer);

        if(!asServer)
            return;

        if (!_roundWins.ContainsKey(winner))
        {
            _roundWins.Add(winner, 0);
        }

        _roundWins[winner]++;

        Debug.Log($"Round Winner: {winner}");

        CheckForGameEnd();

    }

    private void CheckForGameEnd()
    {
        _roundCount++;

        if (_roundCount >= amountOfRounds)
        {
            machine.Next(_roundWins);
            return;
        }

        StartCoroutine(DelayNextState());
    }

    private IEnumerator DelayNextState() 
    { 
        yield return _daley;
        machine.SetState(spawningState);
    }
}
