using PurrNet;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndGameView : View
{

    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private TMP_Text winnerText;
    //[SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        
        InstanceHandler.RegisterInstance(this);


    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<EndGameView>();
    }

    public void SetWinner(PlayerID winner)
    {
        winnerText.text = $"Player {winner.id} wins the game!";
        
        //StartCoroutine(FadeScren(true));

    }

    public IEnumerator FadeScren(bool fadeIn)
    {
        float t = 0;

        while (t < fadeDuration)
        {
            canvasGroup.alpha = fadeIn ? t / fadeDuration : 1 - (t / fadeDuration);

            t += Time.deltaTime;
            yield return null;
        }
    }

    public override void OnShow()
    {
    }
    public override void OnHide()
    {
    }
}
