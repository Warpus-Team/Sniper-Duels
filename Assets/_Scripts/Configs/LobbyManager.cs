using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class LobbyManager : MonoBehaviour
{
    [Header("Create Room Panel")]
    [SerializeField] private TMP_InputField roomNameIF;
    [SerializeField] private TMP_InputField maxPlayersIF;
    [SerializeField] private Button createRoomBtn;

    private string playerId;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log("Signed in as player: " + playerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        createRoomBtn.onClick.AddListener(CreateLobby);
    }

    private async void CreateLobby()
    {
    }
}