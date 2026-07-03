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
    private bool _roundEnding = false;
    private bool _waitingForPlayers = false;

    public GameState CurrentState { get; private set; } = GameState.WaitForPlayers;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        ScoreManager.Instance?.ResetAllScores();
        
        StartCoroutine(StartRoundRoutine());
    }

    // ─────────────────────────────────────────
    // Callbacks de jogadores entrando e saindo
    // ─────────────────────────────────────────

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log($"[Game] {otherPlayer.NickName} saiu da sala.");

        // Para toda lógica de rodada
        StopAllCoroutines();
        _roundEnding = false;
        _waitingForPlayers = true;

        SetState(GameState.WaitForPlayers, _currentRound);
        Debug.Log("[Game] Aguardando novo jogador...");

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return;

        Debug.Log($"[Game] {newPlayer.NickName} entrou. Reiniciando partida...");

        _waitingForPlayers = false;

        // Reset completo sincronizado para todos os clientes
        photonView.RPC(nameof(RPC_ResetGame), RpcTarget.All);
    }

    // ─────────────────────────────────────────
    // Reset completo — roda em TODOS os clientes
    // ─────────────────────────────────────────

    [PunRPC]
    private void RPC_ResetGame()
    {
        Debug.Log("[Game] Reset completo iniciado.");
        StartCoroutine(ResetGameRoutine());
    }

    private IEnumerator ResetGameRoutine()
    {
        // 1. Para qualquer lógica em andamento
        _roundEnding = false;
        _currentRound = 0;
        _alivePlayers.Clear();

        // 2. Destrói o player local atual
        SpawnManager.Instance?.DespawnLocalPlayer();

        // 3. Zera os placares (Custom Properties sincronizam automaticamente)
        if (PhotonNetwork.IsMasterClient)
            ScoreManager.Instance?.ResetAllScores();

        // Aguarda o destroy processar
        yield return new WaitForSeconds(1f);

        // 4. Respawna o player local
        SpawnManager.Instance?.SpawnLocalPlayer();

        // 5. Aguarda o spawn completar
        yield return new WaitForSeconds(2f);

        // 6. Reinicia as rodadas — só MasterClient
        if (PhotonNetwork.IsMasterClient)
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
        Debug.Log($"[Game] Rodada {_currentRound} — {_alivePlayers.Count} jogadores registrados.");
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
            // Remove antes de adicionar para garantir registro único
            p.OnDeath_Server -= OnPlayerDied;
            p.OnDeath_Server += OnPlayerDied;
            _alivePlayers.Add(p);
        }
    }

    // ─────────────────────────────────────────
    // Morte — só MasterClient processa
    // ─────────────────────────────────────────

    public void OnPlayerDied(Player deadPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (_roundEnding) return;
        if (_waitingForPlayers) return; // ← ignora mortes enquanto aguarda jogador

        _alivePlayers.RemoveAll(h =>
            h.photonView.Owner != null &&
            h.photonView.Owner.ActorNumber == deadPlayer.ActorNumber);

        Debug.Log($"[Game] {deadPlayer.NickName} morreu. Vivos: {_alivePlayers.Count}");

        if (_alivePlayers.Count <= 1)
        {
            _roundEnding = true;
            StartCoroutine(EndRoundRoutine());
        }
    }

    // ─────────────────────────────────────────
    // Fim de rodada
    // ─────────────────────────────────────────

    private IEnumerator EndRoundRoutine()
    {
        foreach (var p in _alivePlayers)
            p.OnDeath_Server -= OnPlayerDied;

        SetState(GameState.RoundEnd, _currentRound);

        string roundWinnerName = "";

        // Identifica e pontua o vencedor da rodada
        if (_alivePlayers.Count == 1)
        {
            var roundWinner = _alivePlayers[0].photonView.Owner;
            roundWinnerName = roundWinner.NickName;

            ScoreManager.Instance?.AddKill(roundWinner);

            foreach (var p in PhotonNetwork.PlayerList)
                if (p.ActorNumber != roundWinner.ActorNumber)
                    ScoreManager.Instance?.AddDeath(p);

            Debug.Log($"[Game] {roundWinnerName} venceu a rodada {_currentRound}.");
        }

        yield return new WaitForSeconds(1f);

        // Lê o placar APÓS a propagação e passa diretamente via RPC
        Player pA = GetPhotonPlayerByName("Player A");
        Player pB = GetPhotonPlayerByName("Player B");
        int killsA = pA != null ? ScoreManager.Instance.GetKills(pA) : 0;
        int killsB = pB != null ? ScoreManager.Instance.GetKills(pB) : 0;


        // Notifica a UI de resultado de rodada em TODOS os clientes via RPC
        if (photonView != null)
            photonView.RPC(nameof(RPC_ShowRoundResult), RpcTarget.All,
                roundWinnerName, _currentRound, killsA, killsB);

        yield return new WaitForSeconds(roundEndDelay);

        // Esconde o painel de rodada antes de respawnar
        if (photonView != null)
            photonView.RPC(nameof(RPC_HideRoundResult), RpcTarget.All);

        // Verifica fim de jogo ANTES de respawnar
        if (_currentRound >= totalRounds)
        {
            SetState(GameState.GameEnd, _currentRound);
            yield return new WaitForSeconds(1f); // aguarda sync das Custom Properties
            AnnounceWinner();
            yield break;
        }

        // Respawna todos via RPC para sincronizar os dois clientes
        if (photonView != null)
            photonView.RPC(nameof(RPC_RespawnAll), RpcTarget.All);
        else
            RPC_RespawnAll(); // fallback local

        yield return new WaitForSeconds(1f);
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
            Debug.Log("[Game] Nenhum vencedor encontrado.");
            return;
        }

        Player pA = GetPhotonPlayerByName("Player A");
        Player pB = GetPhotonPlayerByName("Player B");
        int killsA = pA != null ? ScoreManager.Instance.GetKills(pA) : 0;
        int killsB = pB != null ? ScoreManager.Instance.GetKills(pB) : 0;

        Debug.Log($"[Game] Vencedor final: {winner.NickName}");

        // RPC para garantir que AMBOS os clientes vejam a tela de fim de jogo
        if (photonView != null)
            photonView.RPC(
                nameof(RPC_ShowGameEnd), 
                RpcTarget.All, 
                winner.NickName, 
                killsA, killsB
            );

        StartCoroutine(ReturnToMenuRoutine());
    }

    private IEnumerator ReturnToMenuRoutine()
    {
        yield return new WaitForSeconds(5f);

        // Proteção: só sai se ainda estiver em sala
        if (photonView != null)
            photonView.RPC(nameof(RPC_LeaveRoom), RpcTarget.All);
        else if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    private void RPC_LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

    // ─────────────────────────────────────────
    // Sync de estado entre clientes
    // ─────────────────────────────────────────

    private void SetState(GameState state, int round)
    {
        if (!PhotonNetwork.IsConnected)
            return;

        if (!PhotonNetwork.InRoom)
            return;

        if (PhotonNetwork.CurrentRoom == null)
            return;

        if (PhotonNetwork.NetworkClientState != ClientState.Joined)
            return;

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

        Debug.Log("[Game] Novo MasterClient assumiu.");

        if (CurrentState == GameState.RoundRunning)
        {
            _roundEnding = false;
            RegisterAlivePlayers();
        }
    }

    [PunRPC]
    private void RPC_ShowRoundResult(string winnerName, int round, int killsA, int killsB)
    {
        RoundResultUI.Instance?.ShowRoundEnd(winnerName, round, killsA, killsB);
    }

    [PunRPC]
    private void RPC_HideRoundResult()
    {
        RoundResultUI.Instance?.HideRoundResult();
    }

    [PunRPC]
    private void RPC_ShowGameEnd(string winnerName, int killsA, int killsB)
    {
        RoundResultUI.Instance?.ShowGameEnd(winnerName, killsA, killsB);
    }

    private Player GetPhotonPlayerByName(string nickname)
    {
        foreach (var p in PhotonNetwork.PlayerList)
            if (p.NickName == nickname) return p;
        return null;
    }
}