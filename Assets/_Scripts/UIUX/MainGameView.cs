using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainGameView : MonoBehaviour  // ← não herda mais de View
{
    public static MainGameView Instance { get; private set; } // ← singleton novo

    [Header("Configuracoes do Texto")]
    [SerializeField] private TMP_Text healthText;
    
    [Header("Configuracoes da Barrar")]
    [SerializeField] private Slider healthSlider;


    private void Awake()
    {
        Instance = this; // ← substitui InstanceHandler.RegisterInstance
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        healthText.text = $"{currentHealth} / {maxHealth}";

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}