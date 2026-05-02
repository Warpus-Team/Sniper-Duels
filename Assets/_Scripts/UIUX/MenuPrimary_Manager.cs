using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrimary_Manager : MonoBehaviour
{
    [SerializeField] private GameObject PanelMenu;
    [SerializeField] private GameObject PanelOptions;
    [SerializeField] private GameObject PanelCredits;
    [SerializeField] private GameObject PanelPlay;
    [SerializeField] private GameObject PanelPCreate;
    [SerializeField] private GameObject PanelPJoin;
    [SerializeField] private GameObject PanelPNickname;

//////////// Play Scene
    public void OpenPlay(){
        PanelMenu.SetActive(false);
        PanelPlay.SetActive(true);
    }
        public void ClosePlay(){
        PanelPlay.SetActive(false);
        PanelMenu.SetActive(true);
    }

//////////// Multiplayer Scenes
    public void OpenPCreate(){
        PanelPCreate.SetActive(true);
    }
    public void ClosePCreate(){
        PanelPCreate.SetActive(false);
    }
    public void OpenPJoin(){
        PanelPJoin.SetActive(true);
    }
    public void ClosePJoin(){
        PanelPJoin.SetActive(false);
    }

//////////// Player Configuration Scenes
    
    public void OpenPNickname(){
        PanelPNickname.SetActive(true);
    }
    public void ClosePNickname(){
        PanelPNickname.SetActive(false);
    }
    public void OpenOptions(){
        PanelMenu.SetActive(false);
        PanelOptions.SetActive(true);
    }
    public void CloseOptions(){
        PanelOptions.SetActive(false);
        PanelMenu.SetActive(true);
    }

    public void OpenCredits(){
        PanelMenu.SetActive(false);
        PanelCredits.SetActive(true);
    }
    public void CloseCredits(){
        PanelCredits.SetActive(false);
        PanelMenu.SetActive(true);
    }

    public void Exit(){
        Application.Quit();
    }
}
