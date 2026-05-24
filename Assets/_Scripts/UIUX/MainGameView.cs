using UnityEngine;
using TMPro;
using PurrNet;

public class MainGameView : View
{
    [SerializeField] private TMP_Text heatlhText;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<MainGameView>();
    }

    public override void OnHide() {  }

    public override void OnShow() {  }

    public void UpdateHealth(int health)
    {
        heatlhText.text = health.ToString();
    }
}
