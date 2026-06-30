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
        // Só o dono da arma pode atirar
        enabled = photonView.IsMine;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0))
            return;

        if (!Physics.Raycast(
            cameraTransform.position,
            cameraTransform.forward,
            out var hit,
            range,
            hitLayer))
            return;

        if (!hit.transform.TryGetComponent(out PlayerHealth playerHealth))
            return;

        playerHealth.ChangeHealth(-damage);
    }
}