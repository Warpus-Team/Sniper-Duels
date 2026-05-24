using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnState : StateNode
{
    [SerializeField] private PlayerHealth playerPrefab;
    [SerializeField] private List<Transform> spawnPoints = new();

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer)
        {
            return;
        }

        int currentSpawnIndex = 0;

        foreach (var player in networkManager.players)
        {
            var spawnPoint = spawnPoints[currentSpawnIndex];
            var newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            newPlayer.GiveOwnership(player);
            currentSpawnIndex++;

            if (currentSpawnIndex >= spawnPoints.Count)
            {
                currentSpawnIndex = 0;
            }
        }

        //machine.Next();
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
    }
}
