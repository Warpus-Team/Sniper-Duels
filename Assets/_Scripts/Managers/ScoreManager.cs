using PurrNet;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreManager : NetworkBehaviour
{
    [SerializeField] private SyncDictionary<PlayerID, ScoreData> scores = new();

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);

        scores.onChanged += OnScoresChanged; 
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        InstanceHandler.UnregisterInstance<ScoreManager>();

        scores.onChanged -= OnScoresChanged;

    }

    private void OnScoresChanged(SyncDictionaryChange<PlayerID, ScoreData> changer)
    {
        if (InstanceHandler.TryGetInstance(out ScoreBoardView scoreBoardView))
        {
            scoreBoardView.SetData(scores.ToDictionary());
        }
    }

    public void AddKill(PlayerID playerID)
    {
        CheckForDictionaryEntry(playerID);

        var scoreData = scores[playerID];
        scoreData.Kills++;
        scores[playerID] = scoreData;
    }

    public void AddDeath(PlayerID playerID)
    {
        CheckForDictionaryEntry(playerID);

        var scoreData = scores[playerID];
        scoreData.Deaths++;
        scores[playerID] = scoreData;
    }


    public PlayerID GetWinner()
    {
        PlayerID winner = default;

        var highestKill = 0;

        foreach (var score in scores)
        {
            if (score.Value.Kills > highestKill)
            {
                highestKill = score.Value.Kills;
                winner = score.Key;
            }
        }
        return winner;
    }


    private void CheckForDictionaryEntry(PlayerID playerID) 
    {

        if (!scores.ContainsKey(playerID))
        {
            scores.Add(playerID, new ScoreData());
        }
    }

    public struct ScoreData
    { 
        public int Kills;
        public int Deaths;

        public override string ToString()
        {
            return $"{Kills}/{Deaths}";
        }
    }
}
