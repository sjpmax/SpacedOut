using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class PlayerController : MonoBehaviour
{

    [Header("First Person Camera")]
    public float mouseSensitivity = 5f;
    public float verticalLookLimit = 80f;

    private Vector2 lookInput;
    private Camera playerCamera;
    private float verticalRotation = 0f;

    [Header("Space Movement")]
    public float pushForce = 2f;
    public float tetherLength = 10f;

    [Header("Tether Retraction")]
    public float retractSpeed = 8f;

    [Header("Speed Display")]
    public TextMeshProUGUI speedDisplay;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool isShiftHeld = false; // New: Track shift modifier
    private LineRenderer tetherLine;
    private Transform platform;
    private bool isRetracting = false;

    private Vector3 velocity;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        platform = transform.parent;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;  // This is the key fix!
            rb.linearDamping = 0f;          // No air resistance in space
            rb.angularDamping = 0f;   // No rotational drag
        }

        // Set up tether line
        tetherLine = gameObject.AddComponent<LineRenderer>();
        tetherLine.material = new Material(Shader.Find("Sprites/Default"));
        tetherLine.startWidth = 0.05f;
        tetherLine.endWidth = 0.05f;
        tetherLine.positionCount = 2;
        tetherLine.enabled = true;

        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.identity;
        }

        // CRITICAL: Reset vertical rotation to zero
        verticalRotation = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Retract.performed += OnSpacePressed;
        inputActions.Player.VerticalModifier.performed += OnShiftPressed; 
        inputActions.Player.VerticalModifier.canceled += OnShiftReleased; 
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
        HandleSpaceMovement(); 
        HandleTetherLine();
        HandleRetraction();
        HandleMouseLook();

        if (speedDisplay != null)
        {
            float currentSpeed = velocity.magnitude;
            speedDisplay.text = $"Speed: {currentSpeed:F1}";
        }
    }

    void HandleSpaceMovement()
    {
        if (isRetracting) return;

        // Apply momentum (coast after jumping) - NO DAMPING IN SPACE!
        Vector3 newPos = transform.localPosition + velocity * Time.deltaTime;

        // Tether constraint
        float distanceFromPlatform = newPos.magnitude;
        if (distanceFromPlatform > tetherLength)
        {
            newPos = newPos.normalized * tetherLength;
            velocity = Vector3.zero; // Stop when you hit tether limit
        }

        transform.localPosition = newPos;

        // NO DAMPING IN SPACE! Remove this line entirely:
        // velocity *= dampingFactor;
    }

    void HandleTetherLine()
    {
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

    void HandleRetraction()
    {
        if (isRetracting)
        {
            Vector3 directionToPlatform = -transform.localPosition.normalized;
            Vector3 newPos = transform.localPosition + directionToPlatform * retractSpeed * Time.deltaTime;

            if (newPos.magnitude < 0.5f)
            {
                newPos = new Vector3(0, 1.5f, 0);
                isRetracting = false;
                Debug.Log("Back on platform!");
            }

            transform.localPosition = newPos;
        }
    }


    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // New: Handle shift modifier
    private void OnShiftPressed(InputAction.CallbackContext context)
    {
        isShiftHeld = true;
    }

    private void OnShiftReleased(InputAction.CallbackContext context)
    {
        isShiftHeld = false;
    }

    private void OnSpacePressed(InputAction.CallbackContext context)
    {
        Vector3 platformPosition = new Vector3(0, 1f, 0);
        float distanceFromPlatform = Vector3.Distance(transform.localPosition, platformPosition);

        if (distanceFromPlatform < 1f) // Close to platform = JUMP
        {
            Vector3 jumpDirection = CalculateJumpDirection();
            velocity = jumpDirection * pushForce;

            // Debug output to show jump direction
            string directionText = isShiftHeld ? "VERTICAL" : "HORIZONTAL";
            Debug.Log($"Jumped off platform! Direction: {directionText} - {jumpDirection}");
        }
        else // Far from platform = RETRACT
        {
            isRetracting = true;
            velocity = Vector3.zero;
            Debug.Log("Retracting to platform!");
        }
    }

    // New: Calculate jump direction based on input and modifier
    private Vector3 CalculateJumpDirection()
    {
        Vector3 jumpDirection;

        if (moveInput.magnitude > 0.1f)
        {
            if (isShiftHeld)
            {
                // VERTICAL: W/S = Up/Down, A/D = Left/Right
                jumpDirection = new Vector3(moveInput.x, moveInput.y, 0).normalized;
                Debug.Log($"Vertical input - moveInput: {moveInput}, jumpDirection: {jumpDirection}"); // ADD THIS
            }
            else
            {
                // HORIZONTAL: W/S = Forward/Back, A/D = Left/Right  
                jumpDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
                Debug.Log($"Horizontal input - moveInput: {moveInput}, jumpDirection: {jumpDirection}"); // ADD THIS
            }
        }
        else
        {
            jumpDirection = Vector3.forward;
        }

        return jumpDirection;
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    void HandleMouseLook()
    {
        if (Time.time < 0.1f) return; // Skip mouse look for first 0.1 seconds

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