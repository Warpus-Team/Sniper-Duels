using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundResultUI : MonoBehaviour
{
    public static RoundResultUI Instance { get; private set; }

    [Header("Painel de Fim de Rodada")]
    [SerializeField] private GameObject painelRound;
    [SerializeField] private TMP_Text roundTextRound;       // "ROUND X"
    [SerializeField] private TMP_Text resultTextRound;      // "VITÓRIA" / "DERROTA"
    [SerializeField] private TMP_Text nameATextRound;       // nome Player A
    [SerializeField] private TMP_Text scoreATextRound;      // pontuação Player A
    [SerializeField] private TMP_Text nameBTextRound;       // nome Player B
    [SerializeField] private TMP_Text scoreBTextRound;      // pontuação Player B

    [Header("Painel de Fim de Jogo")]
    [SerializeField] private GameObject painelGameEnd;
    [SerializeField] private TMP_Text roundTextGameEnd;     // "ROUND X"
    [SerializeField] private TMP_Text resultTextGameEnd;    // "VITÓRIA" / "DERROTA"
    [SerializeField] private TMP_Text nameATextGameEnd;
    [SerializeField] private TMP_Text scoreATextGameEnd;
    [SerializeField] private TMP_Text nameBTextGameEnd;
    [SerializeField] private TMP_Text scoreBTextGameEnd;
    [SerializeField] private Button btnVoltarLobby;

    [Header("Cores")]
    [SerializeField] private Color corVitoria = new Color(0.4f, 0.6f, 1f);   // azul
    [SerializeField] private Color corDerrota = new Color(1f, 0.3f, 0.3f);   // vermelho

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        painelRound?.SetActive(false);
        painelGameEnd?.SetActive(false);

        btnVoltarLobby?.onClick.AddListener(VoltarLobby);
    }

    // ─────────────────────────────────────────
    // Chamado pelo GameManager via OnRoomPropertiesUpdate
    // ─────────────────────────────────────────

    public void UpdateState(GameManager.GameState state, int round)
    {
        switch (state)
        {
            case GameManager.GameState.RoundRunning:
                // Esconde os dois painéis ao iniciar nova rodada
                painelRound?.SetActive(false);
                painelGameEnd?.SetActive(false);
                break;

            case GameManager.GameState.RoundEnd:
                ShowRoundEnd(round);
                break;

            case GameManager.GameState.GameEnd:
                // Esconde o painel de rodada — o ShowGameEnd cuida do GameEnd
                painelRound?.SetActive(false);
                break;
        }
    }

    // ─────────────────────────────────────────
    // Fim de rodada — mostra por X segundos e some
    // ─────────────────────────────────────────

    private void ShowRoundEnd(int round)
    {
        painelRound?.SetActive(true);

        // Atualiza o número da rodada
        if (roundTextRound != null)
            roundTextRound.text = $"ROUND {round}";

        // Determina se o player local venceu ou perdeu esta rodada
        Player localPlayer = PhotonNetwork.LocalPlayer;
        Player playerA = GetPlayerByName("Player A");
        Player playerB = GetPlayerByName("Player B");

        Debug.Log($"LocalPlayer: {PhotonNetwork.LocalPlayer}");
        Debug.Log($"PlayerA: {playerA}");
        Debug.Log($"PlayerB: {playerB}");
        Debug.Log($"ScoreManager: {ScoreManager.Instance}");
        Debug.Log($"resultTextRound: {resultTextRound}");
        Debug.Log($"painelRound: {painelRound}");
        // Pontuações
        int killsA = playerA != null ? ScoreManager.Instance.GetKills(playerA) : 0;
        int killsB = playerB != null ? ScoreManager.Instance.GetKills(playerB) : 0;

        // Atualiza os textos de pontuação
        AtualizarPlacar(
            scoreATextRound, scoreBTextRound,
            nameATextRound, nameBTextRound,
            killsA, killsB, playerA, playerB
        );

        // Define VITÓRIA ou DERROTA baseado no player local
        bool venceu = (localPlayer.NickName == "Player A" && killsA > killsB) ||
                      (localPlayer.NickName == "Player B" && killsB > killsA);

        if (resultTextRound != null)
        {
            resultTextRound.text  = venceu ? "VITÓRIA" : "DERROTA";
            resultTextRound.color = venceu ? corVitoria : corDerrota;
        }
    }

    // ─────────────────────────────────────────
    // Fim de jogo — chamado pelo GameManager.AnnounceWinner()
    // ─────────────────────────────────────────

    public void ShowGameEnd(string winnerName)
    {
        painelRound?.SetActive(false);
        painelGameEnd?.SetActive(true);

        Player localPlayer = PhotonNetwork.LocalPlayer;
        Player playerA = GetPlayerByName("Player A");
        Player playerB = GetPlayerByName("Player B");

        int killsA = playerA != null ? ScoreManager.Instance.GetKills(playerA) : 0;
        int killsB = playerB != null ? ScoreManager.Instance.GetKills(playerB) : 0;

        // Atualiza placar final
        AtualizarPlacar(
            scoreATextGameEnd, scoreBTextGameEnd,
            nameATextGameEnd, nameBTextGameEnd,
            killsA, killsB, playerA, playerB
        );

        // Define número da rodada final
        if (roundTextGameEnd != null)
            roundTextGameEnd.text = "FIM DE JOGO";

        // Define VITÓRIA ou DERROTA para o player local
        bool venceu = localPlayer.NickName == winnerName;

        if (resultTextGameEnd != null)
        {
            resultTextGameEnd.text  = venceu ? "VITÓRIA" : "DERROTA";
            resultTextGameEnd.color = venceu ? corVitoria : corDerrota;
        }
    }

    // ─────────────────────────────────────────
    // Atualiza os campos de placar nos dois painéis
    // ─────────────────────────────────────────

    private void AtualizarPlacar(
        TMP_Text scoreA, TMP_Text scoreB,
        TMP_Text nameA,  TMP_Text nameB,
        int killsA, int killsB,
        Player playerA, Player playerB)
    {
        if (scoreA != null) scoreA.text = killsA.ToString();
        if (scoreB != null) scoreB.text = killsB.ToString();
        if (nameA  != null) nameA.text  = playerA?.NickName ?? "Player A";
        if (nameB  != null) nameB.text  = playerB?.NickName ?? "Player B";
    }

    // ─────────────────────────────────────────
    // Botão Voltar ao Lobby
    // ─────────────────────────────────────────

    public void VoltarLobby()
    {
        painelGameEnd?.SetActive(false);
        PhotonNetwork.LeaveRoom();
    }

    // ─────────────────────────────────────────
    // Utilitário — busca player pelo nickname
    // ─────────────────────────────────────────

    private Player GetPlayerByName(string nickname)
    {
        foreach (var p in PhotonNetwork.PlayerList)
            if (p.NickName == nickname) return p;
        return null;
    }
}