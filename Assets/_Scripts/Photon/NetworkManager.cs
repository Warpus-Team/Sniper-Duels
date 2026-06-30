using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField] private string battleSceneName;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Garante que todos os clientes carregam a mesma cena
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("[Network] Conectando ao Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[Network] Conectado. Entrando no lobby...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[Network] Lobby OK.");
    }

    // Chamado pelo MenuPrimary_Manager
    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 2 });
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[Network] Entrou na sala: {PhotonNetwork.CurrentRoom.Name} " +
                  $"({PhotonNetwork.CurrentRoom.PlayerCount}/2)");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[Network] {newPlayer.NickName} entrou.");

        // Apenas o MasterClient carrega a cena (AutomaticallySyncScene replica para todos)
        if (PhotonNetwork.IsMasterClient &&
            PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            Debug.Log("[Network] Carregando BattleScene...");
            PhotonNetwork.LoadLevel(battleSceneName);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
        => Debug.LogWarning($"[Network] Falha ao entrar: {message}");

    public override void OnCreateRoomFailed(short returnCode, string message)
        => Debug.LogWarning($"[Network] Falha ao criar: {message}");

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenu");
    }
}