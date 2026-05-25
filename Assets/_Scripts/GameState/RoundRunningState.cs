using PurrNet;
using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundRunningState : StateNode<List<PlayerHealth>>
{

    private List<PlayerID> _players = new();

    public override void Enter(List<PlayerHealth> data, bool asServer)
    {
        base.Enter(data, asServer);

        if (!asServer)
            return;
        
        _players.Clear();

        foreach (var player in data)
        {
            if (player.owner.HasValue)
            {
                _players.Add(player.owner.Value);
            }

            player.OnDeath_Server += OnPlayerDeath;

        }
    }

    public void OnPlayerDeath(PlayerID deadPlayer)
    {

        _players.Remove(deadPlayer);

        if (_players.Count <= 1)
        {
            if (_players.Count == 1) 
            { 
                machine.Next(_players[0]);
            }
            else
            {
                machine.Next();
            }

        }
    }
}
