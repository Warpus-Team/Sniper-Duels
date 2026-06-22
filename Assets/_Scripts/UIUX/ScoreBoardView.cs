using Photon.Pun;
using UnityEngine;

public class ScoreBoardView : MonoBehaviour
{
    public static ScoreBoardView Instance { get; private set; }

    [SerializeField] private Transform entriesParent;
    [SerializeField] private ScoreBoardEntry entryPrefab;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Hide(); // começa escondido
    }

    private void Update()
    {
        // Mantém o mesmo comportamento de Tab do original
        if (Input.GetKeyDown(KeyCode.Tab)) Show();
        if (Input.GetKeyUp(KeyCode.Tab)) Hide();
    }

    // Substitui SetData() — agora lê direto dos Players do Photon
    public void Refresh()
    {
        foreach (Transform child in entriesParent)
            Destroy(child.gameObject);

        foreach (var player in PhotonNetwork.PlayerList)
        {
            var entry = Instantiate(entryPrefab, entriesParent);
            entry.SetData(
                player.NickName,
                ScoreManager.Instance.GetKills(player),
                ScoreManager.Instance.GetDeaths(player)
            );
        }
    }

    public void Show() => canvasGroup.alpha = 1;
    public void Hide() => canvasGroup.alpha = 0;
}