using PurrNet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private SyncVar<int> health = new(100);
    [SerializeField] private int selfLayer, otherLayer;

    public Action<PlayerID> OnDeath_Server;

    public int Health => health;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        var actualLayer = isOwner ? selfLayer : otherLayer;
        SetLayerRecursive(gameObject, actualLayer);

        if (isOwner)
        {
            InstanceHandler.GetInstance<MainGameView>().UpdateHealth(health.value);
            health.onChanged += OnHealthChanged;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        health.onChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int newHealth)
    {
        InstanceHandler.GetInstance<MainGameView>().UpdateHealth(newHealth);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    [ServerRpc(requireOwnership:false)]
    public void ChangeHealth(int amount)
    {
        health.value += amount;

        if(health.value <= 0)
        {
            OnDeath_Server?.Invoke(owner.Value);
            Destroy(gameObject);
        }
    }
}
