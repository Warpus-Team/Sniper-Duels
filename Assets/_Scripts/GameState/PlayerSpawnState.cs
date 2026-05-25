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

        DespawnPlayers();

        var spawnedPlayers = SpawningPlayers();

        machine.Next(spawnedPlayers);
    }

    private List<PlayerHealth> SpawningPlayers() 
    { 
        var spawnedPlayers = new List<PlayerHealth>();

        int currentSpawnIndex = 0;

        foreach (var player in networkManager.players)
        {
            var spawnPoint = spawnPoints[currentSpawnIndex];
            var newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            
            newPlayer.GiveOwnership(player);
            
            spawnedPlayers.Add(newPlayer);

            currentSpawnIndex++;

            if (currentSpawnIndex >= spawnPoints.Count)
            {
                currentSpawnIndex = 0;
            }
        }

        return spawnedPlayers;
    }

    private void DespawnPlayers()
    {
        var allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var player in allPlayers)
        {
            Destroy(player.gameObject);
        }
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
    }
}
