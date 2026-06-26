using Photon.Pun;
using UnityEngine;

public class GunScript : MonoBehaviourPun
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private float range = 20f;
    [SerializeField] private int damage = 10;

    private void Start()
    {
        enabled = photonView.IsMine;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, range, hitLayer))
            return;

        if (!hit.transform.TryGetComponent<PlayerHealth>(out var playerHealth))
            return;

        playerHealth.ChangeHealth(-damage);
    }
}