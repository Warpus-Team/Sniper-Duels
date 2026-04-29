using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonAudioVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum AudioType { Music, SFX }
    public AudioType type;

    public Image targetImage;

    [Header("Sprites")]
    public Sprite onSprite;
    public Sprite offSprite;
    public Sprite hoverOnSprite;
    public Sprite hoverOffSprite;

    private bool isHover = false;

    void Start()
    {
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        bool isMuted = (type == AudioType.Music)
            ? AudioManager.instance.musicSource.mute
            : AudioManager.instance.sfxSource.mute;

        if (isHover)
            targetImage.sprite = isMuted ? hoverOffSprite : hoverOnSprite;
        else
            targetImage.sprite = isMuted ? offSprite : onSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
        UpdateVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
        UpdateVisual();
    }
}