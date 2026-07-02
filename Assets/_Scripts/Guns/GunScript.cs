using System.Collections;
using Photon.Pun;
using UnityEngine;

public class GunScript : MonoBehaviourPun
{
    [Header("Stats")]
    [SerializeField] private float range = 20f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 0.5f;

    [Header("Ammo System")]
    [SerializeField] private int maxAmmo = 15;       // Capacidade máxima do carregador
    [SerializeField] private float reloadTime = 1.5f; // Quanto tempo demora a recarga em segundos
    private int _currentAmmo;                         // Munição atual no pente
    private bool _isReloading = false;                // Bloqueia ações enquanto recarrega

    [Header("Procedural Animation (Relative)")]
    [SerializeField] private Transform weaponTransform;     // O objeto da arma em si que vai mexer
    [SerializeField] private Vector3 reloadPosOffset = new Vector3(0f, -0.2f, 0.05f); // Abaixa (Y) e vai um pouco para frente (Z)
    [SerializeField] private Vector3 reloadRotOffset = new Vector3(25f, 0f, 0f);


    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Animator animator;


    private float _lastFireTime;

    private void Start()
    {
        enabled = photonView.IsMine;

        // Começa a partida com a arma carregada
        _currentAmmo = maxAmmo;
    }

    private void Update()
    {
        // 1. Se estiver recarregando, não faz mais nada no Update (bloqueia tiro e nova recarga)
        if (_isReloading)
            return;

        // 2. Comando manual de recarga (Teclado R)
        // Só recarrega se o pente não estiver cheio
        if (Input.GetKeyDown(KeyCode.R) && _currentAmmo < maxAmmo)
        {
            StartCoroutine(ReloadAnimate());
            return;
        }

        // 3. Verifica o clique do mouse para atirar
        if (!Input.GetMouseButtonDown(0))
            return;

        // 4. Verifica se tem munição. Se não tiver, força a recarga automática
        if (_currentAmmo <= 0)
        {
            Debug.LogWarning("<b>[Arma]</b> Sem munição! Iniciando recarga automática.");
            StartCoroutine(ReloadAnimate());
            return;
        }

        // 5. Controla a cadência de tiro (Fire Rate)
        if (Time.unscaledTime < _lastFireTime + fireRate)
            return;

        _lastFireTime = Time.unscaledTime;
        _currentAmmo--;
        Debug.Log($"<b>[Arma]</b> Tiro disparado! Munição restante: <color=cyan>{_currentAmmo}/{maxAmmo}</color>");

        // Executa os efeitos na rede
        photonView.RPC(nameof(RPC_PlayShotEffect), RpcTarget.All);

        // Desenha a linha de teste no Editor
        Debug.DrawRay(muzzleTransform.position, cameraTransform.forward * range, Color.red, 0.5f);

        // Executa o cálculo físico do tiro
        if (!Physics.Raycast(muzzleTransform.position, cameraTransform.forward, out var hit, range, hitLayer))
            return;

        if (!hit.transform.TryGetComponent<PlayerHealth>(out var playerHealth))
            return;

        playerHealth.ChangeHealth(-damage);
    }

    private IEnumerator ReloadAnimate()
    {
        _isReloading = true;

        // 1. Salva a posição e rotação LOCAIS exatas de onde a arma está AGORA
        Vector3 initialLocalPos = weaponTransform.localPosition;
        Quaternion initialLocalRot = weaponTransform.localRotation;

        // 2. Calcula o destino final baseado nos Offsets (Relativo)
        Vector3 targetLocalPos = initialLocalPos + reloadPosOffset;
        Quaternion targetLocalRot = initialLocalRot * Quaternion.Euler(reloadRotOffset);

        float halfDuration = reloadTime / 2f; // 1.0 segundo para cada fase
        float elapsed = 0f;

        // === FASE 1: ABAIXANDO A ARMA (0s até 1s) ===
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / halfDuration;

            // Interpola suavemente a posição e a rotação locais
            weaponTransform.localPosition = Vector3.Lerp(initialLocalPos, targetLocalPos, percent);
            weaponTransform.localRotation = Quaternion.Slerp(initialLocalRot, targetLocalRot, percent);
            yield return null;
        }

        // Garante que atingiu o valor exato no meio do tempo
        weaponTransform.localPosition = targetLocalPos;
        weaponTransform.localRotation = targetLocalRot;

        // Pequena pausa teórica para simular a troca do pente (opcional)
        _currentAmmo = maxAmmo;

        // Reseta o cronômetro para a volta
        elapsed = 0f;

        // === FASE 2: VOLTANDO PARA A POSIÇÃO ORIGINAL (1s até 2s) ===
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / halfDuration;

            // Faz o caminho inverso
            weaponTransform.localPosition = Vector3.Lerp(targetLocalPos, initialLocalPos, percent);
            weaponTransform.localRotation = Quaternion.Slerp(targetLocalRot, initialLocalRot, percent);
            yield return null;
        }

        // Garante que a arma voltou perfeitamente para o lugar original de mira
        weaponTransform.localPosition = initialLocalPos;
        weaponTransform.localRotation = initialLocalRot;

        _isReloading = false;
    }

  
    [PunRPC]
    private void RPC_PlayShotEffect()
    {
        if (muzzleFlash != null)
            muzzleFlash.Play();
    }
}