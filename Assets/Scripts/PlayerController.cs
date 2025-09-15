using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Space Movement")]
    public float pushForce = 5f;
    public float tetherLength = 10f;
    public float grabRange = 2f;
    public float dampingFactor = 0.98f; // Slight slowdown over time

    [Header("Tether Retraction")]
    public float retractSpeed = 8f;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private LineRenderer tetherLine;
    private Transform platform;
    private bool isRetracting = false;

    private Vector3 velocity; // Player's current momentum
    private bool justPushed = false;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        platform = transform.parent;

        // Set up tether line
        tetherLine = gameObject.AddComponent<LineRenderer>();
        tetherLine.material = new Material(Shader.Find("Sprites/Default"));
        tetherLine.startWidth = 0.05f;
        tetherLine.endWidth = 0.05f;
        tetherLine.positionCount = 2;
        tetherLine.enabled = true;
    }

    void OnEnable()
    {
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Retract.performed += OnSpacePressed;  // One action for both
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
        CheckForDebris();
    }

    void HandleSpaceMovement()
    {
        if (isRetracting) return;

        // Apply momentum (coast after jumping)
        Vector3 newPos = transform.localPosition + velocity * Time.deltaTime;

        // Tether constraint
        float distanceFromPlatform = newPos.magnitude;
        if (distanceFromPlatform > tetherLength)
        {
            newPos = newPos.normalized * tetherLength;
            velocity = Vector3.zero; // Stop when you hit tether limit
        }

        transform.localPosition = newPos;

        // Slight damping
        velocity *= 0.99f;
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
                // Position player on top of platform instead of at center
                newPos = new Vector3(0, 1.5f, 0); // 1 unit above platform center
                isRetracting = false;
                Debug.Log("Back on platform!");
            }

            transform.localPosition = newPos;
        }
    }

    void CheckForDebris()
    {
        Collider[] nearbyDebris = Physics.OverlapSphere(transform.position, grabRange);

        foreach (Collider debris in nearbyDebris)
        {
            if (debris.CompareTag("Debris"))
            {
                Debug.Log("caught debris");
                CollectDebris(debris.gameObject);
                break;
            }
        }
    }

    void CollectDebris(GameObject debris)
    {
        Debug.Log("Collected: " + debris.name + "!");
        Destroy(debris);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnRetractStart(InputAction.CallbackContext context)
    {
        isRetracting = true;
        Debug.Log("Retracting!");
    }

    private void OnRetractStop(InputAction.CallbackContext context)
    {
        isRetracting = false;
        Debug.Log("Retract stopped!");
    }

    private void OnSpacePressed(InputAction.CallbackContext context)
    {
        // Check if close to platform (accounting for height)
        Vector3 platformPosition = new Vector3(0, 1f, 0); // Where player should be when "on platform"
        float distanceFromPlatform = Vector3.Distance(transform.localPosition, platformPosition);

        if (distanceFromPlatform < 1f) // Close to platform = JUMP
        {
            Vector3 jumpDirection;
            if (moveInput.magnitude > 0.1f)
            {
                jumpDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            }
            else
            {
                jumpDirection = Vector3.forward;
            }

            velocity = jumpDirection * pushForce;
            Debug.Log("Jumped off platform!");
        }
        else // Far from platform = RETRACT
        {
            isRetracting = true;
            velocity = Vector3.zero;
            Debug.Log("Retracting to platform!");
        }
    }
}