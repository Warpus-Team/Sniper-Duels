using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PurrNet;
using Unity.VisualScripting;

public class ViewManager : MonoBehaviour
{
    [SerializeField] private List<View> allViews = new();
    [SerializeField] private View defaultView;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);

        foreach (var view in allViews)
        {
            HideViewInternal(view);
        }
        ShowViewInternal(defaultView);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<GameViewManager>();
    }


    public void ShowView<T>(bool hideOthers = true) where T : View
    {
        foreach (var view in allViews)
        {
            if (view.GetType() == typeof(T))
            {
                ShowViewInternal(view);
            }
            else
            {
                if (hideOthers)
                {
                    HideViewInternal(view);
                }
            }

        }
    }

    public void HideView<T>() where T : View
    {
        foreach (var view in allViews)
        {
            if (view.GetType() == typeof(T))
            {
                HideViewInternal(view);
            }
        }
    }

    private void ShowViewInternal(View view)
    {
        view.canvasGroup.alpha = 1;
        view.OnShow();
    }

    private void HideViewInternal(View view)
    {
        view.canvasGroup.alpha = 0;
        view.OnHide();
    }
}

public abstract class View : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public abstract void OnShow();
    public abstract void OnHide(); 
}

