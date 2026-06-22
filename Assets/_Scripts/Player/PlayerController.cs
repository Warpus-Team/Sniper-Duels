// Scripts/Player/PlayerController.cs
using Photon.Pun;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviourPun  // ← era NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera playerCamera;
    [SerializeField] private Animator animator; // ← PhotonAnimatorView cuida da sync; aqui só o Animator local
    [SerializeField] private List<Renderer> renderers = new();

    private CharacterController _characterController;
    private Vector3 _velocity;
    private float _verticalRotation;
    private bool _isCursorLocked = true;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();

        // ← era OnSpawned() + isOwner
        bool isLocalPlayer = photonView.IsMine;

        enabled = isLocalPlayer;
        playerCamera.gameObject.SetActive(isLocalPlayer);

        if (isLocalPlayer)
        {
            foreach (var rend in renderers)
                rend.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

            LockCursor();
        }

        if (playerCamera == null)
            enabled = false;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        HandleCursorInput();
        if (!_isCursorLocked) return;
        HandleMovement();
        HandleRotation();
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleMovement()
    {
        bool isGrounded = IsGrounded();
        if (isGrounded && _velocity.y < 0) _velocity.y = -2f;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = Vector3.ClampMagnitude(transform.right * h + transform.forward * v, 1f);
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        _characterController.Move(moveDir * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);

        // PhotonAnimatorView sincroniza os parâmetros automaticamente pelo componente
        animator.SetFloat("Forward", v);
        animator.SetFloat("Sideways", h);
        animator.SetFloat("Jump", _velocity.y);
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        _verticalRotation = Mathf.Clamp(_verticalRotation - mouseY, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private bool IsGrounded() =>
        Physics.Raycast(transform.position + Vector3.up * 0.03f, Vector3.down, groundCheckDistance);

    private void HandleCursorInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) UnlockCursor();
        if (Input.GetMouseButtonDown(0) && !_isCursorLocked) LockCursor();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _isCursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _isCursorLocked = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.03f, Vector3.down * groundCheckDistance);
    }
#endif
}