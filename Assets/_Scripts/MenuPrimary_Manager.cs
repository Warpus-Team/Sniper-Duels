using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrimary_Manager : MonoBehaviour
{
    [SerializeField] private GameObject PanelMenu;
    [SerializeField] private GameObject PanelOptions;
    [SerializeField] private GameObject PanelCredits;
    [SerializeField] private string scenaNamePlay;
    
    public void LoadSceneGameplay() //Mudar para a cena de jogo
    {
        SceneManager.LoadScene(scenaNamePlay);
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
