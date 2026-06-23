using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrimary_Manager : MonoBehaviourPunCallbacks
{
    [Header("Painéis — mesma estrutura original")]
    [SerializeField] private GameObject PanelMenu;
    [SerializeField] private GameObject PanelOptions;
    [SerializeField] private GameObject PanelCredits;
    [SerializeField] private GameObject PanelPlay;
    [SerializeField] private GameObject PanelPCreate;
    [SerializeField] private GameObject PanelPJoin;
    [SerializeField] private GameObject PanelPNickname;

    [SerializeField] private string gameSceneName;

    public void LoadGameplay()
    {
        //SceneManager.LoadScene(gameSceneName);
    }

    public void OpenPlay() { PanelMenu.SetActive(false); PanelPlay.SetActive(true); }
    public void ClosePlay() { PanelMenu.SetActive(true); PanelPlay.SetActive(false); }
    public void OpenPCreate() => PanelPCreate.SetActive(true);
    public void ClosePCreate() => PanelPCreate.SetActive(false);
    public void OpenPJoin() => PanelPJoin.SetActive(true);
    public void ClosePJoin() => PanelPJoin.SetActive(false);
    public void OpenPNickname() => PanelPNickname.SetActive(true);
    public void ClosePNickname() => PanelPNickname.SetActive(false);
    public void OpenOptions() { PanelMenu.SetActive(false); PanelOptions.SetActive(true); }
    public void CloseOptions() { PanelOptions.SetActive(false); PanelMenu.SetActive(true); }
    public void OpenCredits() { PanelMenu.SetActive(false); PanelCredits.SetActive(true); }
    public void CloseCredits() { PanelCredits.SetActive(false); PanelMenu.SetActive(true); }
    public void Exit() => Application.Quit();

}