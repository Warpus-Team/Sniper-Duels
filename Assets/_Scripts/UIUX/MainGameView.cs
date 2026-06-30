using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainGameView : MonoBehaviourPun  // ← não herda mais de View
{
    public static MainGameView Instance { get; private set; } // ← singleton novo

    [Header("Configuracoes do Texto")]
    [SerializeField] private TMP_Text healthText;
    
    [Header("Configuracoes da Barrar")]
    [SerializeField] private Slider healthSlider;

    [Header("Canvas")]
    [SerializeField] private GameObject canvasRoot;

    private void Awake()
    {
        Debug.Log($"[MainGameView] IsMine: {photonView.IsMine} | CanvasRoot: {canvasRoot}");

        // Cada player agora tem seu próprio Canvas dentro do prefab
        if (photonView.IsMine)
        {
            Instance = this;
            if (canvasRoot != null)
                canvasRoot.SetActive(true);
        }
        else
        {
            // Esconde o HUD dos outros jogadores
            if (canvasRoot != null)
                canvasRoot.SetActive(false);
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        healthText.text = $"{currentHealth} / {maxHealth}";

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}