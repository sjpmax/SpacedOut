using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("First Person Camera")]
    public float mouseSensitivity = 5f;
    public float verticalLookLimit = 80f;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float pushForce = 5f;
    public float tetherLength = 10f;

    [Header("Tether Retraction")]
    public float retractSpeed = 8f;

    [Header("Speed Display")]
    public TextMeshProUGUI speedDisplay;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isCtrlHeld = false;
    private LineRenderer tetherLine;
    private Transform platform;
    private bool isRetracting = false;
    private Camera playerCamera;
    private float verticalRotation = 0f;

    private Vector3 velocity;
    private bool isOnPlatform = true;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        platform = transform.parent;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        SetupTetherLine();
        SetupCamera();
    }

    void SetupTetherLine()
    {
        tetherLine = gameObject.AddComponent<LineRenderer>();
        tetherLine.material = new Material(Shader.Find("Sprites/Default"));
        tetherLine.startWidth = 0.05f;
        tetherLine.endWidth = 0.05f;
        tetherLine.positionCount = 2;
        tetherLine.enabled = true;
    }

    void SetupCamera()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.identity;
        }
        verticalRotation = 0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Retract.performed += OnSpacePressed;
        inputActions.Player.VerticalModifier.performed += OnCtrlPressed;
        inputActions.Player.VerticalModifier.canceled += OnCtrlReleased;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;
        inputActions.Player.Escape.performed += OnEscapePressed;
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        UpdatePlatformStatus();
        HandleMovement();
        HandleTetherLine();
        HandleRetraction();
        HandleMouseLook();
        UpdateSpeedDisplay();
    }

    void UpdatePlatformStatus()
    {
        Vector3 platformCenter = new Vector3(0, 1.5f, 0);
        float distanceFromCenter = Vector3.Distance(transform.localPosition, platformCenter);
        isOnPlatform = distanceFromCenter < 1.5f;
    }

    void HandleMovement()
    {
        if (isRetracting) return;

        if (isOnPlatform && velocity.magnitude < 0.1f)  // ← Add velocity check
        {
            // Walking on platform (only if not actively jumping)
            HandleWalking();
        }
        else
        {
            // Floating in space (includes right after jump)
            HandleSpaceMovement();
        }
    }

    void HandleWalking()
    {
        // Disable walking when Ctrl is held (for vertical jump mode)
        if (isCtrlHeld) return;

        if (moveInput.magnitude > 0.1f)
        {
            // Camera-relative walking
            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;

            // Flatten to platform plane
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 walkDirection = (forward * moveInput.y + right * moveInput.x).normalized;
            Vector3 newPosition = transform.localPosition + walkDirection * walkSpeed * Time.deltaTime;

            transform.localPosition = newPosition;
        }
    }

    void HandleSpaceMovement()
    {
        // Apply momentum - coast after jumping
        Vector3 newPos = transform.localPosition + velocity * Time.deltaTime;

        // Tether constraint
        float distanceFromPlatform = newPos.magnitude;
        if (distanceFromPlatform > tetherLength)
        {
            // Soft constraint - slow down near edge
            Vector3 pullDirection = -newPos.normalized;
            velocity += pullDirection * 5f * Time.deltaTime;

            // Hard limit
            if (distanceFromPlatform > tetherLength * 1.05f)
            {
                newPos = newPos.normalized * tetherLength;
                velocity *= 0.5f;
            }
        }

        transform.localPosition = newPos;
    }

    void HandleTetherLine()
    {
        if (!isOnPlatform)
        {
            tetherLine.enabled = true;
            tetherLine.SetPosition(0, transform.position);
            tetherLine.SetPosition(1, platform.position);

            // Visual feedback
            float tension = transform.localPosition.magnitude / tetherLength;
            if (tension > 0.8f)
                tetherLine.material.color = Color.red;
            else if (tension > 0.5f)
                tetherLine.material.color = Color.yellow;
            else
                tetherLine.material.color = Color.white;
        }
        else
        {
            tetherLine.enabled = false;
        }
    }

    void HandleRetraction()
    {
        if (!isRetracting) return;

        Vector3 targetPos = new Vector3(0, 1.5f, 0);
        Vector3 directionToPlatform = (targetPos - transform.localPosition).normalized;
        Vector3 newPos = transform.localPosition + directionToPlatform * retractSpeed * Time.deltaTime;

        if (Vector3.Distance(newPos, targetPos) < 0.5f)
        {
            transform.localPosition = targetPos;
            isRetracting = false;
            velocity = Vector3.zero;
            Debug.Log("Back on platform!");
        }
        else
        {
            transform.localPosition = newPos;
        }
    }

    void UpdateSpeedDisplay()
    {
        if (speedDisplay != null)
        {
            string status = isOnPlatform ? "ON PLATFORM" : "FLOATING";
            string ctrlStatus = isCtrlHeld ? "CTRL: ON" : "CTRL: OFF"; // Add this
            speedDisplay.text = $"Speed: {velocity.magnitude:F1}\n{status}\n{ctrlStatus}";
        }
    }

    void HandleMouseLook()
    {
        if (Time.time < 0.1f) return;

        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(0, mouseX, 0);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
    }

    // Input handlers
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnCtrlPressed(InputAction.CallbackContext context)
    {
        Debug.Log("CTRL PRESSED!"); // Add this
        isCtrlHeld = true;
    }

    private void OnCtrlReleased(InputAction.CallbackContext context)
    {
        Debug.Log("CTRL RELEASED!"); // Add this
        isCtrlHeld = false;
    }

    private void OnSpacePressed(InputAction.CallbackContext context)
    {
        Debug.Log($"=== SPACE PRESSED ===");
        Debug.Log($"isOnPlatform: {isOnPlatform}");
        Debug.Log($"isRetracting: {isRetracting}");
        Debug.Log($"isCtrlHeld: {isCtrlHeld}");
        Debug.Log($"moveInput: {moveInput}");
        Debug.Log($"transform.localPosition: {transform.localPosition}");

        if (isOnPlatform && !isRetracting)
        {
            Vector3 jumpDirection = CalculateJumpDirection();
            Debug.Log($"Jump direction calculated: {jumpDirection}");

            velocity = jumpDirection * pushForce;
            Debug.Log($"Velocity set to: {velocity}");

            string directionText = isCtrlHeld ? "VERTICAL" : "HORIZONTAL";
            Debug.Log($"Jumped! {directionText} - {jumpDirection}");
        }
        else if (!isOnPlatform && !isRetracting)
        {
            isRetracting = true;
            velocity = Vector3.zero;
            Debug.Log("Retracting!");
        }
        else
        {
            Debug.Log("No action taken - conditions not met");
        }
    }

    private Vector3 CalculateJumpDirection()
    {
        Debug.Log($"CalculateJumpDirection - moveInput: {moveInput}, isCtrlHeld: {isCtrlHeld}");

        if (moveInput.magnitude > 0.1f)
        {
            if (isCtrlHeld)
            {
                Vector3 result = (transform.right * moveInput.x + transform.up * moveInput.y).normalized;
                Debug.Log($"Ctrl jump: {result}");
                return result;
            }
            else
            {
                Vector3 result = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
                Debug.Log($"Normal jump: {result}");
                return result;
            }
        }
        else
        {
            Debug.Log($"No input jump: transform.forward = {transform.forward}");
            return transform.forward;
        }
    }
    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}