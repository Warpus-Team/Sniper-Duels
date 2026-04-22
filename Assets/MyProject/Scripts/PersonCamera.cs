using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform player;         // arrasta o Player aqui
    public Vector3 offset = new Vector3(0.7f, 1.6f, 0f); // ombro direito

    [Header("Orbit")]
    public float mouseSensitivity = 3f;
    public float pitchMin = -30f;
    public float pitchMax = 60f;

    [Header("Zoom")]
    public float distance = 4f;      // distância normal
    public float minDistance = 1f;   // distância mínima ao colidir
    public float smoothSpeed = 10f;

    [Header("Collision")]
    public LayerMask collisionLayers;
    public float collisionRadius = 0.2f;

    private float yaw = 0f;    // rotação horizontal (mouse X)
    private float pitch = 0f;  // rotação vertical   (mouse Y)

    void Start()
    {
        yaw = player.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        HandleOrbit();
        HandleCollision();
        RotatePlayer();
    }

    void HandleOrbit()
    {
        // Lê o mouse
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // Calcula onde a câmera deve ficar
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivotWorld = player.position + Vector3.up * offset.y;
        Vector3 offsetLocal = rotation * new Vector3(offset.x, 0f, -distance);

        // Aplica com suavidade
        transform.position = Vector3.Lerp(
            transform.position,
            pivotWorld + offsetLocal,
            smoothSpeed * Time.deltaTime
        );
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            rotation,
            smoothSpeed * Time.deltaTime
        );
    }

    void HandleCollision()
    {
        // Ponto de origem (ombro do player)
        Vector3 pivot = player.position + Vector3.up * offset.y;

        // Direção da câmera até o pivot
        Vector3 dir = transform.position - pivot;
        float targetDist = distance;

        // Raycast verifica se tem parede no caminho
        if (Physics.SphereCast(pivot, collisionRadius, dir.normalized, out RaycastHit hit, distance, collisionLayers))
        {
            targetDist = Mathf.Clamp(hit.distance, minDistance, distance);
        }

        // Reposiciona a câmera na distância correta
        transform.position = pivot + dir.normalized * targetDist;
    }

    void RotatePlayer()
    {
        // Player rotaciona suavemente para onde a câmera aponta (só eixo Y)
        Quaternion targetRot = Quaternion.Euler(0f, yaw, 0f);
        player.rotation = Quaternion.Lerp(
            player.rotation,
            targetRot,
            smoothSpeed * Time.deltaTime
        );
    }

}
