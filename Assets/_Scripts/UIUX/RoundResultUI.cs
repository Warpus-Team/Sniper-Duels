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
    [SerializeField] private TMP_Text roundTextRound;
    [SerializeField] private TMP_Text resultTextRound;
    [SerializeField] private TMP_Text scoreATextRound;
    [SerializeField] private TMP_Text scoreBTextRound;
    [SerializeField] private TMP_Text nameATextRound;
    [SerializeField] private TMP_Text nameBTextRound;

    [Header("Painel de Fim de Jogo")]
    [SerializeField] private GameObject painelGameEnd;
    [SerializeField] private TMP_Text roundTextGameEnd;
    [SerializeField] private TMP_Text resultTextGameEnd;
    [SerializeField] private TMP_Text scoreATextGameEnd;
    [SerializeField] private TMP_Text scoreBTextGameEnd;
    [SerializeField] private TMP_Text nameATextGameEnd;
    [SerializeField] private TMP_Text nameBTextGameEnd;
    [SerializeField] private Button btnVoltarLobby;

    [Header("HUD do Player (desabilitar durante placar)")]
    private GameObject canvasPlayerHUD;

    [Header("Cores")]
    [SerializeField] private Color corVitoria = new Color(0.4f, 0.6f, 1f);
    [SerializeField] private Color corDerrota = new Color(1f, 0.3f, 0.3f);

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        painelRound?.SetActive(false);
        painelGameEnd?.SetActive(false);

        btnVoltarLobby?.onClick.AddListener(VoltarLobby);
    }

    // ─────────────────────────────────────────
    // Chamado via RPC pelo GameManager
    // ─────────────────────────────────────────

    public void ShowRoundEnd(string winnerName, int round, int killsA, int killsB)
    {
        painelRound?.SetActive(true);
        painelGameEnd?.SetActive(false);

        // Desabilita HUD do player durante o placar
        SetPlayerHUD(false);

        if (roundTextRound != null)
            roundTextRound.text = $"ROUND {round}";

        // Determina vitória/derrota baseado em quem o GameManager disse que venceu
        bool venceu = PhotonNetwork.LocalPlayer.NickName == winnerName;

        if (resultTextRound != null)
        {
            resultTextRound.text = venceu ? "VITÓRIA" : "DERROTA";
            resultTextRound.color = venceu ? corVitoria : corDerrota;
        }

        // Usa os scores passados diretamente — sem ler do ScoreManager
        if (scoreATextRound != null) scoreATextRound.text = killsA.ToString();
        if (scoreBTextRound != null) scoreBTextRound.text = killsB.ToString();

        Player pA = GetPlayerByName("Player A");
        Player pB = GetPlayerByName("Player B");
        if (nameATextRound != null) nameATextRound.text = pA?.NickName ?? "Player A";
        if (nameBTextRound != null) nameBTextRound.text = pB?.NickName ?? "Player B";
    }

    public void HideRoundResult()
    {
        painelRound?.SetActive(false);

        // Reabilita HUD do player ao voltar para a rodada
        SetPlayerHUD(true);
    }

    public void ShowGameEnd(string winnerName, int killsA, int killsB)
    {
        painelRound?.SetActive(false);
        painelGameEnd?.SetActive(true);

        // Desabilita HUD durante tela final
        SetPlayerHUD(false);

        bool venceu = PhotonNetwork.LocalPlayer.NickName == winnerName;

        if (roundTextGameEnd != null)
            roundTextGameEnd.text = "FIM DE JOGO";

        if (resultTextGameEnd != null)
        {
            resultTextGameEnd.text = venceu ? "VITÓRIA" : "DERROTA";
            resultTextGameEnd.color = venceu ? corVitoria : corDerrota;
        }

        if (scoreATextGameEnd != null) scoreATextGameEnd.text = killsA.ToString();
        if (scoreBTextGameEnd != null) scoreBTextGameEnd.text = killsB.ToString();

        Player pA = GetPlayerByName("Player A");
        Player pB = GetPlayerByName("Player B");
        if (nameATextGameEnd != null) nameATextGameEnd.text = pA?.NickName ?? "Player A";
        if (nameBTextGameEnd != null) nameBTextGameEnd.text = pB?.NickName ?? "Player B";
    }

    // ─────────────────────────────────────────
    // Chamado pelo OnRoomPropertiesUpdate (compatibilidade)
    // ─────────────────────────────────────────

    public void UpdateState(GameManager.GameState state, int round)
    {
        // Este método agora serve só de fallback
        // A lógica principal está nos RPCs acima
        if (state == GameManager.GameState.RoundRunning)
            HideRoundResult();
    }

    // ─────────────────────────────────────────
    // Utilitários
    // ─────────────────────────────────────────

    private void AtualizarPlacar(
        TMP_Text scoreA, TMP_Text scoreB,
        TMP_Text nameA, TMP_Text nameB)
    {
        Player pA = GetPlayerByName("Player A");
        Player pB = GetPlayerByName("Player B");

        int killsA = pA != null && ScoreManager.Instance != null
            ? ScoreManager.Instance.GetKills(pA) : 0;
        int killsB = pB != null && ScoreManager.Instance != null
            ? ScoreManager.Instance.GetKills(pB) : 0;

        if (scoreA != null) scoreA.text = killsA.ToString();
        if (scoreB != null) scoreB.text = killsB.ToString();
        if (nameA != null) nameA.text = pA?.NickName ?? "Player A";
        if (nameB != null) nameB.text = pB?.NickName ?? "Player B";
    }

    private void SetPlayerHUD(bool ativo)
    {
        // Tenta pegar o CanvasPlayer do inspector
        if (canvasPlayerHUD != null)
        {
            canvasPlayerHUD.SetActive(ativo);
            return;
        }

        // Fallback: busca o MainGameView na cena
        if (MainGameView.Instance != null)
            MainGameView.Instance.gameObject.SetActive(ativo);
    }

    private Player GetPlayerByName(string nickname)
    {
        foreach (var p in PhotonNetwork.PlayerList)
            if (p.NickName == nickname) return p;
        return null;
    }

    public void VoltarLobby()
    {
        painelGameEnd?.SetActive(false);
        SetPlayerHUD(true);

        // Proteção: só sai se estiver em sala
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}