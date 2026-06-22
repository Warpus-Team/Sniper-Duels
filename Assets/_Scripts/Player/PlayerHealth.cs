// Scripts/Player/PlayerHealth.cs
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int selfLayer;
    [SerializeField] private int otherLayer;

    public event Action<Player> OnDeath_Server;

    private int _health;
    public int Health => _health;

    // ─────────────────────────────────────────
    // Inicialização
    // ─────────────────────────────────────────

    private void Start()
    {
        _health = maxHealth;

        int layer = photonView.IsMine ? selfLayer : otherLayer;
        SetLayerRecursive(gameObject, layer);

        if (photonView.IsMine)
            MainGameView.Instance?.UpdateHealth(_health);
    }

    // ─────────────────────────────────────────
    // Dano — chamado pelo GunScript
    // ─────────────────────────────────────────

    public void ChangeHealth(int amount)
    {
        // Envia RPC para o dono do player que sofreu o dano
        photonView.RPC(
            nameof(RPC_ChangeHealth),
            photonView.Owner,
            amount,
            PhotonNetwork.LocalPlayer.ActorNumber
        );
    }

    [PunRPC]
    private void RPC_ChangeHealth(int amount, int killerActorNumber)
    {
        _health += amount;
        _health = Mathf.Clamp(_health, 0, maxHealth);

        // Atualiza HUD (roda no dono do player, é seguro)
        MainGameView.Instance?.UpdateHealth(_health);

        if (_health <= 0)
            Die(killerActorNumber);
    }

    // ─────────────────────────────────────────
    // Morte
    // ─────────────────────────────────────────

    private void Die(int killerActorNumber)
    {
        // Avisa o MasterClient para processar rodada e placar
        photonView.RPC(
            nameof(RPC_NotifyDeath),
            RpcTarget.MasterClient,
            photonView.Owner.ActorNumber,
            killerActorNumber
        );

        gameObject.SetActive(false);
    }

    [PunRPC]
    private void RPC_NotifyDeath(int deadActorNumber, int killerActorNumber)
    {
        // Roda apenas no MasterClient (equivalente ao "asServer" do PurrNet)
        Player dead = GetPlayerByActor(deadActorNumber);
        Player killer = GetPlayerByActor(killerActorNumber);

        ScoreManager.Instance?.AddKill(killer);
        ScoreManager.Instance?.AddDeath(dead);

        OnDeath_Server?.Invoke(dead);
        GameManager.Instance?.OnPlayerDied(dead);
    }

    // ─────────────────────────────────────────
    // Respawn (chamado pelo SpawnManager)
    // ─────────────────────────────────────────

    public void Respawn()
    {
        _health = maxHealth;
        gameObject.SetActive(true);

        if (photonView.IsMine)
            MainGameView.Instance?.UpdateHealth(_health);
    }

    // ─────────────────────────────────────────
    // Sync de vida para todos os clientes
    // Substitui o SyncVar<int> do PurrNet
    // ─────────────────────────────────────────

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(_health);
        else
            _health = (int)stream.ReceiveNext();
    }

    // ─────────────────────────────────────────
    // Utilitários
    // ─────────────────────────────────────────

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    private Player GetPlayerByActor(int actorNumber)
    {
        foreach (var p in PhotonNetwork.PlayerList)
            if (p.ActorNumber == actorNumber) return p;
        return null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Mostra o layer configurado no Inspector durante edição
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            $"SelfLayer: {selfLayer} | OtherLayer: {otherLayer}"
        );
    }
#endif
}