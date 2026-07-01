using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        WaitForPlayers,
        RoundRunning,
        RoundEnd,
        GameEnd
    }

    private const string PROP_STATE = "gs";
    private const string PROP_ROUND = "rd";

    [Header("Configurações")]
    [SerializeField] private int totalRounds = 3;
    [SerializeField] private float roundEndDelay = 3f;

    private List<PlayerHealth> _alivePlayers = new();
    private int _currentRound = 0;

    // ← flag para evitar EndRound duplicado
    private bool _roundEnding = false;

    public GameState CurrentState { get; private set; } = GameState.WaitForPlayers;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Apenas o MasterClient controla o fluxo de rodadas
        if (!PhotonNetwork.IsMasterClient) return;
        
        StartCoroutine(StartRoundRoutine());
    }

    // ─────────────────────────────────────────
    // Início de rodada
    // ─────────────────────────────────────────

    private IEnumerator StartRoundRoutine()
    {
        _roundEnding = false;
        _currentRound++;

        Debug.Log($"[Game] Iniciando rodada {_currentRound}...");

        SetState(GameState.RoundRunning, _currentRound);

        yield return new WaitForSeconds(2f);

        RegisterAlivePlayers();

        Debug.Log($"[Game] Rodada {_currentRound} iniciada com {_alivePlayers.Count} jogadores.");
    }

    private void RegisterAlivePlayers()
    {
        _alivePlayers.Clear();

        var allPlayers = FindObjectsByType<PlayerHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        Debug.Log($"[Game] RegisterAlivePlayers encontrou: {allPlayers.Length} players");

        foreach (var p in allPlayers)
        {
            p.OnDeath_Server -= OnPlayerDied;
            p.OnDeath_Server += OnPlayerDied;
            _alivePlayers.Add(p);
        }
    }

    // ─────────────────────────────────────────
    // Morte de player
    // ─────────────────────────────────────────

    public void OnPlayerDied(Player deadPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (_roundEnding) return;

        _alivePlayers.RemoveAll(h =>
            h.photonView.Owner != null &&
            h.photonView.Owner.ActorNumber == deadPlayer.ActorNumber);

        Debug.Log($"[Game] {deadPlayer.NickName} morreu. Vivos: {_alivePlayers.Count}");

        if (_alivePlayers.Count <= 1) { 
            _roundEnding = true;
            StartCoroutine(EndRoundRoutine());
        }
    }

    // ─────────────────────────────────────────
    // Fim de rodada
    // ─────────────────────────────────────────

    private IEnumerator EndRoundRoutine()
    {
        var _roundEndDelay = 1f;

        SetState(GameState.RoundEnd, _currentRound);

        // Identifica o vencedor da rodada
        if (_alivePlayers.Count == 1)
        {
            var roundWinner = _alivePlayers[0].photonView.Owner;
            ScoreManager.Instance?.AddKill(roundWinner);
            Debug.Log($"[Game] {roundWinner.NickName} venceu a rodada {_currentRound}.");
        }

        yield return new WaitForSeconds(roundEndDelay);

        // Verifica se o jogo acabou ANTES de respawnar
        if (_currentRound >= totalRounds)
        {
            SetState(GameState.GameEnd, _currentRound);
            yield return new WaitForSeconds(_roundEndDelay);
            AnnounceWinner();
            yield break;
        }

        // Respawna os dois jogadores via RPC para garantir sincronização
        if (photonView != null) {
            photonView.RPC(nameof(RPC_RespawnAll), RpcTarget.All);
        }
        else {
            RPC_RespawnAll(); // fallback local
        }
        
        yield return new WaitForSeconds(_roundEndDelay);

        StartCoroutine(StartRoundRoutine());
    }

    // ─────────────────────────────────────────
    // Respawn sincronizado — roda em TODOS os clientes
    // ─────────────────────────────────────────

    [PunRPC]
    private void RPC_RespawnAll()
    {
        SpawnManager.Instance?.RespawnLocalPlayer();
        Debug.Log("[Game] RespawnAll executado.");
    }

    // ─────────────────────────────────────────
    // Fim de jogo
    // ─────────────────────────────────────────

    private void AnnounceWinner()
    {
        Player winner = ScoreManager.Instance?.GetWinner();
        if (winner == null)
        {
            Debug.Log("[Game] Nenhum vencedor.");
            return;
        }
        Debug.Log($"[Game] Vencedor final: {winner.NickName}");
        RoundResultUI.Instance?.ShowGameEnd(winner.NickName);
    }

    // ─────────────────────────────────────────
    // Sync entre clientes
    // ─────────────────────────────────────────

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

    public override void OnRoomPropertiesUpdate(Hashtable changedProps)
    {
        if (changedProps.TryGetValue(PROP_STATE, out object stateVal))
        {
            var newState = (GameState)(int)stateVal;
            int round = changedProps.TryGetValue(PROP_ROUND, out object r)
                ? (int)r : _currentRound;

            CurrentState = newState;
            _currentRound = round;

            RoundResultUI.Instance?.UpdateState(newState, round);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("[Game] Novo MasterClient assumiu. Reiniciando controle...");

        if (CurrentState == GameState.RoundRunning)
        {
            _roundEnding = false;
            RegisterAlivePlayers();
        }
    }
}