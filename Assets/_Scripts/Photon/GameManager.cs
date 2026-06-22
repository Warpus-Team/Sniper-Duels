using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; // ← manter só uma
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable; // ← adicionar esta linha


public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    // ─── Mesmos estados da sua StateMachine original ───
    public enum GameState
    {
        WaitForPlayers,   // WaitForPlayersState
        PlayerSpawning,   // PlayerSpawnState
        RoundRunning,     // RoundRunningState
        RoundEnd,         // RoundEndState
        GameEnd           // GameEndState
    }

    // Room Property keys (substitui as propriedades síncronas do PurrNet)
    private const string PROP_STATE = "gs";
    private const string PROP_ROUND = "rd";

    [Header("Configurações (era RoundEndState)")]
    [SerializeField] private int totalRounds = 3;
    [SerializeField] private float roundEndDelay = 3f;

    [Header("Spawn (era PlayerSpawnState)")]
    [SerializeField] private string playerPrefabName = "Player"; // deve estar em Resources/
    [SerializeField] private List<Transform> spawnPoints = new();

    private List<PlayerHealth> _alivePlayers = new();
    private int _currentRound = 0;

    public GameState CurrentState { get; private set; } = GameState.WaitForPlayers;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Somente o MasterClient dirige os estados (era "asServer" no PurrNet)
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(WaitForPlayersRoutine());
    }

    // ════════════════════════════════════════
    // ESTADOS
    // ════════════════════════════════════════

    // ── WaitForPlayersState ──
    private IEnumerator WaitForPlayersRoutine()
    {
        SetState(GameState.WaitForPlayers, 0);
        // Aguarda 2 jogadores (era: while(networkManager.players.Count < minPlayers))
        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.PlayerCount >= 2);
        StartCoroutine(SpawnPlayersRoutine());
    }

    // ── PlayerSpawnState ──
    private IEnumerator SpawnPlayersRoutine()
    {
        SetState(GameState.PlayerSpawning, _currentRound);

        DespawnPlayers(); // era DespawnPlayers() no PlayerSpawnState
        _alivePlayers.Clear();

        yield return null; // frame de respiro antes de spawnar

        int spawnIndex = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (spawnIndex >= spawnPoints.Count) spawnIndex = 0;
            Transform sp = spawnPoints[spawnIndex++];

            // PhotonNetwork.Instantiate → equivalente ao Instantiate + GiveOwnership do PurrNet
            var go = PhotonNetwork.Instantiate(playerPrefabName, sp.position, sp.rotation);
            var health = go.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.OnDeath_Server += OnPlayerDied; // mesmo evento do seu RoundRunningState
                _alivePlayers.Add(health);
            }
        }

        SetState(GameState.RoundRunning, _currentRound);
    }

    // ── RoundRunningState: OnPlayerDeath ──
    public void OnPlayerDied(Player deadPlayer)
    {
        // Remove da lista de vivos (era _players.Remove(deadPlayer))
        _alivePlayers.RemoveAll(h =>
            h.photonView.Owner.ActorNumber == deadPlayer.ActorNumber);

        if (_alivePlayers.Count <= 1)
            StartCoroutine(EndRoundRoutine());
    }

    // ── RoundEndState ──
    private IEnumerator EndRoundRoutine()
    {
        _currentRound++;
        SetState(GameState.RoundEnd, _currentRound);

        Debug.Log($"[Game] Rodada {_currentRound} encerrada.");

        // Verifica fim de jogo (era CheckForGameEnd())
        if (_currentRound >= totalRounds)
        {
            SetState(GameState.GameEnd, _currentRound);
            AnnounceWinner();
            yield break;
        }

        yield return new WaitForSeconds(roundEndDelay); // era _delay = new WaitForSeconds(3f)
        StartCoroutine(SpawnPlayersRoutine());           // era machine.SetState(spawningState)
    }

    // ── GameEndState ──
    private void AnnounceWinner()
    {
        Player winner = ScoreManager.Instance?.GetWinner();
        if (winner == null)
        {
            Debug.Log("[Game] Nenhum vencedor encontrado.");
            return;
        }
        Debug.Log($"[Game] Vencedor: {winner.NickName}");
        
        // Notifica a UI de fim de jogo
        RoundResultUI.Instance?.ShowGameEnd(winner.NickName);
    }

    private void DespawnPlayers()
    {
        // era FindObjectsByType<PlayerHealth>(...) + Destroy
        var all = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in all)
        {
            p.OnDeath_Server -= OnPlayerDied;
            if (p.photonView.IsMine)
                PhotonNetwork.Destroy(p.gameObject);
        }
    }

    // ════════════════════════════════════════
    // SYNC DE ESTADO (Room Properties)
    // ════════════════════════════════════════

    // Substitui o SetRoomState — todos os clientes recebem via callback
    private void SetState(GameState state, int round)
    {
        CurrentState = state;
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
        {
            { PROP_STATE, (int)state },
            { PROP_ROUND, round }
        });
    }

    // Callback Photon → dispara em TODOS os clientes (era OnRoomPropertiesUpdate)
    public override void OnRoomPropertiesUpdate(Hashtable changedProps)
    {
        if (changedProps.TryGetValue(PROP_STATE, out object stateVal))
        {
            var newState = (GameState)(int)stateVal;
            int round = changedProps.TryGetValue(PROP_ROUND, out object r) ? (int)r : _currentRound;

            CurrentState = newState;
            _currentRound = round;

            // Notifica a UI (mesmo papel do OnRoomPropertiesUpdate anterior)
            RoundResultUI.Instance?.UpdateState(newState, round);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Se o MasterClient sair, o novo assume o controle
        if (PhotonNetwork.IsMasterClient)
            Debug.Log("[Game] Novo MasterClient assumiu o controle.");
    }
}