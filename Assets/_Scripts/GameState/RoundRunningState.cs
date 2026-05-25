using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundRunningState : StateNode<List<PlayerHealth>>
{
    private int _playerAlive;

    public override void Enter(List<PlayerHealth> data, bool asServer)
    {
        base.Enter(data, asServer);

        if (!asServer)
            return;
        
        _playerAlive = data.Count;

        foreach (var player in data)
        {
            player.OnDeath_Server += OnPlayerDeath;
        }
    }

    public void OnPlayerDeath(PlayerHealth deadPlayer)
    {
        deadPlayer.OnDeath_Server -= OnPlayerDeath;

        _playerAlive--;

        if (_playerAlive <= 1)
        {
            Debug.Log("Someone won the round!!");
            //machine.Next();
        }
    }
}
