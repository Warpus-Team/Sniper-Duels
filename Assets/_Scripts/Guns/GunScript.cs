using Photon.Pun;
using UnityEngine;

public class GunScript : MonoBehaviourPun
{ 
    [Header("Stats")]
    [SerializeField] private float range = 20f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 0.5f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
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

        photonView.RPC(nameof(RPC_PlayShotEffect), RpcTarget.All);

        // 1. VISUAL DEBUG: Desenha uma linha vermelha na aba "Scene" que dura 0.5 segundos
        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * range, Color.red, 0.5f);

        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, range, hitLayer))
        {
            // 3. CONSOLE DEBUG: O raio não bateu em nada (errou o alvo ou fora do alcance)
            Debug.LogWarning($"<b>[{gameObject.name}]</b> O tiro foi disparado, mas não atingiu nada dentro do alcance de {range}m.");
            return;
        }

        // 4. CONSOLE DEBUG: O raio colidiu com alguma coisa na Layer configurada
        Debug.Log($"<b>[{gameObject.name}]</b> O raio atingiu o objeto: <color=yellow>{hit.transform.name}</color> na coordenada {hit.point}.");

        if (!hit.transform.TryGetComponent<PlayerHealth>(out var playerHealth))
        {
            return;
        }

        // 6. CONSOLE DEBUG: Sucesso total! Acertou um inimigo válido
        Debug.Log($"<color=green><b>[SUCESSO]</b></color> <b>[{gameObject.name}]</b> Acertou o jogador {hit.transform.name}! Aplicando {-damage} de vida.");

        playerHealth.ChangeHealth(-damage);
    }

    [PunRPC]
    private void RPC_PlayShotEffect()
    {
        if (muzzleFlash != null)
            muzzleFlash.Play();
    }
}