using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager Instance { get; private set; }

    [Header("Lista de Salas")]
    [SerializeField] private Transform roomListParent;
    [SerializeField] private RoomItemUI roomItemPrefab;

    [Header("Criar Sala")]
    [SerializeField] private TMP_InputField createRoomInput;

    [Header("Entrar por Código")]
    [SerializeField] private TMP_InputField joinCodeInput;

    [Header("Painel de Código (visível para quem criou)")]
    [SerializeField] private GameObject painelCodigoSala;
    [SerializeField] private TMP_Text codigoSalaText;

    [Header("Configurações")]
    [SerializeField] private string gameSceneName;

    //[Header("Status")]
    //[SerializeField] private TMP_Text statusText;

    private List<GameObject> _roomItems = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        painelCodigoSala?.SetActive(false);

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

    }

    // ─────────────────────────────────────────
    // Callbacks Photon
    // ─────────────────────────────────────────

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var obj in _roomItems)
            Destroy(obj);
        _roomItems.Clear();

        foreach (var room in roomList)
        {
            if (room.RemovedFromList) continue;

            var item = Instantiate(roomItemPrefab, roomListParent);
            item.Setup(room);
            _roomItems.Add(item.gameObject);
        }
    }

    public override void OnCreatedRoom()
    {
        // Exibe o código da sala para quem criou
        string codigo = PhotonNetwork.CurrentRoom.Name;
        painelCodigoSala?.SetActive(true);

        if (codigoSalaText != null)
            codigoSalaText.text = codigo;

        Debug.Log($"[Lobby] Sala criada. Código: {codigo}");
    }

    public override void OnJoinedRoom()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        PhotonNetwork.NickName = playerCount == 1 ? "Player A" : "Player B";

        Debug.Log($"[Lobby] Entrou como {PhotonNetwork.NickName}");

        // ← ADICIONAR ISTO TEMPORARIAMENTE PARA TESTE COM 1 JOGADOR
        //if (PhotonNetwork.IsMasterClient)
        //    PhotonNetwork.LoadLevel(gameSceneName);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("[Lobby] Player B entrou. Iniciando...");

        if (PhotonNetwork.IsMasterClient &&
            PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
        => Debug.LogWarning($"[Lobby] Erro ao entrar: {message}");

    public override void OnCreateRoomFailed(short returnCode, string message)
        => Debug.LogWarning($"[Lobby] Erro ao criar: {message}");

    public override void OnLeftRoom()
    {
        painelCodigoSala?.SetActive(false);
        Debug.Log("[Lobby] Saiu da sala.");
    }

    // ─────────────────────────────────────────
    // Botões da UI
    // ─────────────────────────────────────────

    public void CreateRoom()
    {

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            return;
        }

        // Usa o campo de texto ou gera código aleatório
        string codigo = createRoomInput != null
            ? createRoomInput.text.Trim().ToUpper()
            : GerarCodigo();

        if (string.IsNullOrEmpty(codigo))
            codigo = GerarCodigo();

        PhotonNetwork.NickName = "Player A";

        PhotonNetwork.CreateRoom(codigo, new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true
        });

        Debug.Log($"[Lobby] Criando sala com código: {codigo}");
    }

    // Entrar pelo código digitado manualmente
    public void JoinByCode()
    {
        if (joinCodeInput == null) return;

        string codigo = joinCodeInput.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(codigo)) return;

        PhotonNetwork.NickName = "Player B";
        PhotonNetwork.JoinRoom(codigo);

        Debug.Log($"[Lobby] Entrando pelo código: {codigo}");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // ─────────────────────────────────────────
    // Utilitário
    // ─────────────────────────────────────────

    // Gera código no formato: PALAVRA-NÚMERO ex: SALA-4821
    private string GerarCodigo()
    {
        string[] prefixos = { "SALA", "DUELO", "ARENA", "MIRA", "SNIPER" };
        string prefixo = prefixos[Random.Range(0, prefixos.Length)];
        int numero = Random.Range(1000, 9999);
        return $"{prefixo}-{numero}";
    }

    //private void SetStatus(string msg)
    //{
    //    if (statusText != null)
    //        statusText.text = msg;

        //Debug.Log($"[Lobby] {msg}");
    //}
}
