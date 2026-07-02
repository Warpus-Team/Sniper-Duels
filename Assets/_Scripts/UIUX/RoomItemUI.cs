using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playersText;

    private RoomInfo _roomInfo;

    public void Setup(RoomInfo room)
    {
        _roomInfo = room;
        roomNameText.text = room.Name;
        playersText.text = $"VAGAS: {room.PlayerCount}/{room.MaxPlayers}";
    }

    public void JoinRoom()
    {
        if (_roomInfo == null) return;

        //  Proteção: só entra se estiver no Master Server
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("[RoomItem] Photon não está pronto ainda.");
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[RoomItem] Ainda em sala. Saindo antes de entrar em outra.");
            PhotonNetwork.LeaveRoom();
            return;
        }

        PhotonNetwork.NickName = "Player B";
        PhotonNetwork.JoinRoom(_roomInfo.Name);
        Debug.Log($"[RoomItem] Entrando na sala: {_roomInfo.Name}");
    }
}