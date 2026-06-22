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

    [Header("Campos de texto")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_InputField createRoomInput;
    [SerializeField] private TMP_InputField joinRoomInput;
    [SerializeField] private TMP_Text statusText;

    [SerializeField] private string gameSceneName;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.IsConnected)
        {
            SetStatus("Conectando...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // ─── Callbacks Photon ───

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        SetStatus("Conectado!");
    }

    public override void OnJoinedRoom()
    {
        SetStatus($"Na sala: {PhotonNetwork.CurrentRoom.Name} " +
                  $"({PhotonNetwork.CurrentRoom.PlayerCount}/2)");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetStatus($"{newPlayer.NickName} entrou. " +
                  $"{PhotonNetwork.CurrentRoom.PlayerCount}/2");

        if (PhotonNetwork.IsMasterClient &&
            PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            PhotonNetwork.LoadLevel(gameSceneName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
        => SetStatus($"Erro ao entrar: {message}");

    public override void OnCreateRoomFailed(short returnCode, string message)
        => SetStatus($"Erro ao criar sala: {message}");

    public override void OnLeftRoom()
        => SceneManager.LoadScene("MainMenu");

    // ─── Nickname ───

    public void ConfirmNickname()
    {
        string nick = nicknameInput.text.Trim();
        if (string.IsNullOrEmpty(nick)) return;
        PhotonNetwork.NickName = nick;
        ClosePNickname();
    }

    // ─── Criar / Entrar em Sala ───

    public void CreateRoom()
    {
        string roomName = createRoomInput.text.Trim();
        if (string.IsNullOrEmpty(roomName)) return;
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 2 });
        SetStatus($"Criando sala '{roomName}'...");
    }

    public void JoinRoom()
    {
        string roomName = joinRoomInput.text.Trim();
        if (string.IsNullOrEmpty(roomName)) return;
        PhotonNetwork.JoinRoom(roomName);
        SetStatus($"Entrando em '{roomName}'...");
    }

    // ─── Navegação de painéis — igual ao original ───

    public void LoadGameplay()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenPlay() { PanelMenu.SetActive(false); PanelPlay.SetActive(true); }
    public void ClosePlay() { PanelPlay.SetActive(false); PanelMenu.SetActive(true); }
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

    // ─── Utilitário ───

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"[Menu] {msg}");
    }
}