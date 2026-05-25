using PurrNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunScript : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private float range = 20f;
    [SerializeField] private int damage = 10;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0))
            return;

        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, range, hitLayer))
            return;
        if(!hit.transform.TryGetComponent(out PlayerHealth playerHealth))
            return;

        playerHealth.ChangeHealth(-damage);
    }
}
