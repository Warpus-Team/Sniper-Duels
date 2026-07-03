using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;

public class ScoreManager : MonoBehaviourPunCallbacks
{
    public static ScoreManager Instance { get; private set; }

    // Substitui SyncDictionary — Custom Properties ficam em cada Player do Photon
    private const string KEY_KILLS = "kills";
    private const string KEY_DEATHS = "deaths";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─── Substitui AddKill(PlayerID) ───
    public void AddKill(Player player)
    {
        int current = GetKills(player);
        player.SetCustomProperties(new Hashtable { { KEY_KILLS, current + 1 } });
    }

    public void AddDeath(Player player)
    {
        int current = GetDeaths(player);
        player.SetCustomProperties(new Hashtable { { KEY_DEATHS, current + 1 } });
    }

    public int GetKills(Player player)
    {
        if (player.CustomProperties.TryGetValue(KEY_KILLS, out object val))
            return (int)val;
        return 0;
    }

    public int GetDeaths(Player player)
    {
        if (player.CustomProperties.TryGetValue(KEY_DEATHS, out object val))
            return (int)val;
        return 0;
    }

    public Player GetWinner()
    {
        Player winner = null;
        int topKills = -1;
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            int k = GetKills(p);
            if (k > topKills) { topKills = k; winner = p; }
        }
        return winner;
    }

    public void ResetAllScores()
    {
        if (PhotonNetwork.PlayerList == null || PhotonNetwork.PlayerList.Length == 0)
        {
            Debug.LogWarning("[Score] Nenhum player para resetar.");
            return;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            p.SetCustomProperties(new Hashtable
            {
                { KEY_KILLS,  0 },
                { KEY_DEATHS, 0 }
            });
        }
        Debug.Log("[Score] Pontuacoes zeradas.");
    }

    // Substitui scores.onChanged — Photon dispara em TODOS os clientes automaticamente
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(KEY_KILLS) || changedProps.ContainsKey(KEY_DEATHS))
            ScoreBoardView.Instance?.Refresh();
    }
}