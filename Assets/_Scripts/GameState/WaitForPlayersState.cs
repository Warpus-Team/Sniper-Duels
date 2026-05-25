using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForPlayersState : StateNode
{
    [SerializeField] private int minplayers = 2;
    
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer)
            return;

        StartCoroutine(WaitForPlayers());
    }
    private IEnumerator WaitForPlayers()
    {
        while (networkManager.players.Count < minplayers)
            yield return null;
        machine.Next();
    }
}
