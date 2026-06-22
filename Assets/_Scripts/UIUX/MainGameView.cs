using UnityEngine;
using TMPro;

public class MainGameView : MonoBehaviour  // ← não herda mais de View
{
    public static MainGameView Instance { get; private set; } // ← singleton novo

    [SerializeField] private TMP_Text heatlhText;

    private void Awake()
    {
        Instance = this; // ← substitui InstanceHandler.RegisterInstance
    }

    public void UpdateHealth(int health)
    {
        heatlhText.text = health.ToString();
    }
}