using TMPro;
using UnityEngine;

public class RoundResultUI : MonoBehaviour
{
    public static RoundResultUI Instance { get; private set; }

    [Header("Painel de resultado de rodada")]
    [SerializeField] private GameObject roundResultPanel;
    [SerializeField] private TMP_Text roundResultText;

    [Header("Painel de fim de jogo")]
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TMP_Text winnerText;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        roundResultPanel.SetActive(false);
        gameEndPanel.SetActive(false);
    }

    public void UpdateState(GameManager.GameState state, int round)
    {
        switch (state)
        {
            case GameManager.GameState.RoundRunning:
                roundResultPanel.SetActive(false);
                break;

            case GameManager.GameState.RoundEnd:
                roundResultText.text = $"Rodada {round} encerrada!";
                roundResultPanel.SetActive(true);
                break;

            case GameManager.GameState.GameEnd:
                roundResultPanel.SetActive(false);
                break;
        }
    }

    public void ShowGameEnd(string winnerName)
    {
        gameEndPanel.SetActive(true);
        winnerText.text = $"{winnerName} venceu!";
    }
}