using Photon.Pun;
using UnityEngine;

public class GunScript : MonoBehaviourPun
{
    [Header("Stats")]
    [SerializeField] private float range = 20f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 0.5f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform; // Mantido para saber a direção do olhar
    [SerializeField] private Transform muzzleTransform; // NOVO: A ponta do cano da arma
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private ParticleSystem muzzleFlash;

    private float _lastFireTime;

    private void Start()
    {
        enabled = photonView.IsMine;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (Time.unscaledTime < _lastFireTime + fireRate)
            return;

        _lastFireTime = Time.unscaledTime;

        // O RPC continua funcionando perfeitamente porque o script está no mesmo objeto do PhotonView
        photonView.RPC(nameof(RPC_PlayShotEffect), RpcTarget.All);

        Debug.DrawRay(muzzleTransform.position, cameraTransform.forward * range, Color.red, 0.5f);

        // Executa o Raycast físico usando os mesmos parâmetros do Debug acima
        if (!Physics.Raycast(muzzleTransform.position, cameraTransform.forward, out var hit, range, hitLayer))
            return;

        if (!hit.transform.TryGetComponent<PlayerHealth>(out var playerHealth))
            return;

        playerHealth.ChangeHealth(-damage);
    }

    [PunRPC]
    private void RPC_PlayShotEffect()
    {
        if (muzzleFlash != null)
            muzzleFlash.Play();
    }
}