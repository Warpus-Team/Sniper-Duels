using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float jumpForce = 1f;
    public float gravity = -25f;

    private CharacterController controller;
    private Vector3 velocity;

    [Header("Camera")]
    //public Transform cameraTarget;
    public float mouseSensitivity = 500f;
    private float xRotation = 0f;
    private float playerYRotation = 0f;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        //LockCursor();
    }

    void Update()
    {
        //HandleMouseLook();
        HandleMovement(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        //cameraTarget.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement(float h, float v)
    {
        Vector3 move = transform.right * h + transform.forward * v;

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump"))
            Jump();

        velocity.y += gravity * Time.deltaTime;

        controller.Move((move * moveSpeed + Vector3.up * velocity.y) * Time.deltaTime);
    }

    void Jump()
    {
        if (controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}