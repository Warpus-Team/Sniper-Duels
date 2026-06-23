using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager Instance;

    [Header("Campos")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_InputField createRoomInput;
    [SerializeField] private TMP_InputField joinRoomInput;

    [Header("Status")]
    [SerializeField] private TMP_Text statusText;

    [SerializeField] private string gameSceneName;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            SetStatus("Conectando...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        SetStatus("Conectado!");
    }

    public override void OnJoinedRoom()
    {
        SetStatus(
            $"Na sala: {PhotonNetwork.CurrentRoom.Name} " +
            $"({PhotonNetwork.CurrentRoom.PlayerCount}/2)"
        );
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetStatus(
            $"{newPlayer.NickName} entrou. " +
            $"{PhotonNetwork.CurrentRoom.PlayerCount}/2"
        );

        if (PhotonNetwork.IsMasterClient &&
            PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetStatus($"Erro ao entrar: {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetStatus($"Erro ao criar sala: {message}");
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ConfirmNickname()
    {
        string nick = nicknameInput.text.Trim();

        if (string.IsNullOrEmpty(nick))
            return;

        PhotonNetwork.NickName = nick;
    }

    public void CreateRoom()
    {
        string roomName = createRoomInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
            return;

        PhotonNetwork.CreateRoom(
            roomName,
            new RoomOptions { MaxPlayers = 2 }
        );

        SetStatus($"Criando sala '{roomName}'...");
    }

    public void JoinRoom()
    {
        string roomName = joinRoomInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
            return;

        PhotonNetwork.JoinRoom(roomName);

        SetStatus($"Entrando em '{roomName}'...");
    }

    private void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;

        Debug.Log($"[Lobby] {msg}");
    }
}