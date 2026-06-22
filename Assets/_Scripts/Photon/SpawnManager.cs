using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Points (índice 0 = ActorNumber 1, índice 1 = ActorNumber 2)")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Nome do prefab (deve estar em Assets/Resources/)")]
    [SerializeField] private string playerPrefabName = "Player";

    private PlayerHealth _localPlayer;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        //SpawnLocalPlayer();
    }

    public override void OnJoinedRoom()
    {
        SpawnLocalPlayer();
    }


    // ─────────────────────────────────────────────────
    // Spawn inicial
    // ─────────────────────────────────────────────────
    private void SpawnLocalPlayer()
    {
        // ActorNumber 1 → [0] Team A1
        // ActorNumber 2 → [1] Team B1
        // ActorNumber 3 → [2] Team A2
        // ActorNumber 4 → [3] Team B2
        // ActorNumber começa em 1; subtrai 1 para virar índice do array
        int index = Mathf.Clamp(
            PhotonNetwork.LocalPlayer.ActorNumber - 1,
            0,
            spawnPoints.Length - 1
        );

        Transform spawnPoint = spawnPoints[index];

        Debug.Log($"[Spawn] Instanciando player no ponto {index} " +
                  $"({spawnPoint.position})");

        GameObject playerGO = PhotonNetwork.Instantiate(
            playerPrefabName,
            spawnPoint.position,
            spawnPoint.rotation
        );

        _localPlayer = playerGO.GetComponent<PlayerHealth>();

        // Registra o evento de morte para o GameManager (sistema de rodadas)
        if (_localPlayer != null)
            _localPlayer.OnDeath_Server += OnLocalPlayerDied;
    }

    // ─────────────────────────────────────────────────
    // Respawn (chamado pelo GameManager entre rodadas)
    // ─────────────────────────────────────────────────
    public void RespawnLocalPlayer()
    {
        if (_localPlayer == null)
        {
            SpawnLocalPlayer();
            return;
        }

        int index = Mathf.Clamp(
            PhotonNetwork.LocalPlayer.ActorNumber - 1,
            0,
            spawnPoints.Length - 1
        );

        Transform spawnPoint = spawnPoints[index];

        // Teleporta e restaura via PhotonView para sincronizar posição
        _localPlayer.transform.position = spawnPoint.position;
        _localPlayer.transform.rotation = spawnPoint.rotation;
        _localPlayer.Respawn();

        Debug.Log($"[Spawn] Respawn no ponto {index}.");
    }

    // ─────────────────────────────────────────────────
    // Derruba todos os players ao trocar de rodada
    // (só o MasterClient chama via GameManager)
    // ─────────────────────────────────────────────────
    public void DespawnAll()
    {
        var allPlayers = FindObjectsByType<PlayerHealth>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var p in allPlayers)
        {
            if (p.photonView.IsMine)
            {
                p.OnDeath_Server -= OnLocalPlayerDied;
                PhotonNetwork.Destroy(p.gameObject);
            }
        }

        _localPlayer = null;
    }

    private void OnLocalPlayerDied(Player deadPlayer)
    {
        // Apenas registra — o GameManager cuida da lógica de rodada
        Debug.Log($"[Spawn] Player local morreu: {deadPlayer?.NickName}");
    }

}