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

    public GameState CurrentState { get; private set; } = GameState.WaitForPlayers;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(StartRoundRoutine());
    }

    // ─────────────────────────────────────────
    // Início de rodada
    // ─────────────────────────────────────────

    private IEnumerator StartRoundRoutine()
    {
        _currentRound++;
        SetState(GameState.RoundRunning, _currentRound);

        // Aguarda um frame para os players já existirem na cena
        yield return new WaitForSeconds(0.5f);

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

        _alivePlayers.RemoveAll(h =>
            h.photonView.Owner != null &&
            h.photonView.Owner.ActorNumber == deadPlayer.ActorNumber);

        Debug.Log($"[Game] {deadPlayer.NickName} morreu. Vivos: {_alivePlayers.Count}");

        if (_alivePlayers.Count <= 1)
            StartCoroutine(EndRoundRoutine());
    }

    // ─────────────────────────────────────────
    // Fim de rodada
    // ─────────────────────────────────────────

    private IEnumerator EndRoundRoutine()
    {
        SetState(GameState.RoundEnd, _currentRound);

        if (_alivePlayers.Count == 1)
        {
            var winner = _alivePlayers[0].photonView.Owner;
            ScoreManager.Instance?.AddKill(winner);
            Debug.Log($"[Game] {winner.NickName} venceu a rodada {_currentRound}.");
        }

        if (_currentRound >= totalRounds)
        {
            yield return new WaitForSeconds(roundEndDelay);
            SetState(GameState.GameEnd, _currentRound);
            AnnounceWinner();
            yield break;
        }

        yield return new WaitForSeconds(roundEndDelay);

        // Respawna todos via SpawnManager
        SpawnManager.Instance?.RespawnLocalPlayer();

        StartCoroutine(StartRoundRoutine());
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
        if (PhotonNetwork.IsMasterClient)
            Debug.Log("[Game] Novo MasterClient assumiu o controle.");
    }
}