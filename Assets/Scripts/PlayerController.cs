using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float lookSensitivity = .2f;
    public bool magnetBootsOn = true;
    public float bootAttractionForce = 400f;
    public float AttractionCheckDistance = 5f;
    public float rotationAlignmentSpeed = 30f;

    // NEW: Stability settings
    public float groundNormalSmoothSpeed = 40f; // Smooth out normal changes
    public float surfaceChangeThreshold = 0.85f; // Lower = more sensitive to changes

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Rigidbody rb;
    private Camera playerCamera;
    private float cameraPitch = 0f;
    private float cameraYaw = 0f;
    private bool isGrounded = true;
    private Vector3 groundNormal = Vector3.up;
    private Vector3 lastGroundNormal = Vector3.up;
    private Vector3 targetGroundNormal = Vector3.up; // NEW: Target to smooth towards
    private Vector3 smoothedGroundNormal = Vector3.up; // NEW: Smoothed normal

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();
        

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
        inputActions.Player.ToggleGravityBoots.performed += ctx => ToggleGravityBoots();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        cameraYaw += mouseX;
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        Quaternion localYaw = Quaternion.AngleAxis(cameraYaw, Vector3.up);
        Quaternion localPitch = Quaternion.AngleAxis(cameraPitch, Vector3.right);

        playerCamera.transform.localRotation = localYaw * localPitch;
    }

    void FixedUpdate()
    {
        CheckGround();

        // NEW: Smooth the ground normal changes
        SmoothGroundNormal();

        AlignToSurface();

        // Calculate alignment once for both force and movement
        float alignmentDot = Vector3.Dot(transform.up, smoothedGroundNormal);

        if (magnetBootsOn && isGrounded && alignmentDot > 0.5f)
        {
            rb.AddForce(-transform.up * bootAttractionForce, ForceMode.Acceleration);
        }

        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        Vector3 forward = Vector3.ProjectOnPlane(cameraForward, transform.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cameraRight, transform.up).normalized;

        if (alignmentDot > 0.5f) // Use same threshold for consistency
        {
            Vector3 move = right * moveInput.x + forward * moveInput.y;
            rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * move);
        }
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void ToggleGravityBoots()
    {
        magnetBootsOn = !magnetBootsOn;
    }

    private void CheckGround()
    {
        RaycastHit bestHit = default;
        bool foundGround = false;
        float closestDistance = float.MaxValue;

        // Check all directions and find the CLOSEST hit
        TryFindClosestHit(-transform.up, ref bestHit, ref foundGround, ref closestDistance);
        TryFindClosestHit(transform.forward, ref bestHit, ref foundGround, ref closestDistance);
        TryFindClosestHit(-transform.forward, ref bestHit, ref foundGround, ref closestDistance);
        TryFindClosestHit(transform.right, ref bestHit, ref foundGround, ref closestDistance);
        TryFindClosestHit(-transform.right, ref bestHit, ref foundGround, ref closestDistance);

        // Diagonal checks
        TryFindClosestHit((transform.forward - transform.up).normalized, ref bestHit, ref foundGround, ref closestDistance);
        TryFindClosestHit((-transform.forward - transform.up).normalized, ref bestHit, ref foundGround, ref closestDistance);
        TryFindClosestHit((transform.right - transform.up).normalized, ref bestHit, ref foundGround, ref closestDistance);
        TryFindClosestHit((-transform.right - transform.up).normalized, ref bestHit, ref foundGround, ref closestDistance);

        if (foundGround)
        {
            isGrounded = true;
            targetGroundNormal = bestHit.normal;
            Debug.DrawRay(transform.position, bestHit.normal * 2f, Color.red);
        }
        else
        {
            isGrounded = false;
        }
    }

    private void TryFindClosestHit(Vector3 direction, ref RaycastHit bestHit, ref bool foundGround, ref float closestDistance)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, AttractionCheckDistance))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    bestHit = hit;
                    foundGround = true;
                }
            }
        }
    }

    // NEW: Smooth out ground normal changes to prevent jittering
    private void SmoothGroundNormal()
    {
        if (!isGrounded)
        {
            smoothedGroundNormal = Vector3.up;
            return;
        }

        // Smoothly interpolate toward the target normal
        smoothedGroundNormal = Vector3.Slerp(
            smoothedGroundNormal,
            targetGroundNormal,
            Time.fixedDeltaTime * groundNormalSmoothSpeed
        );

        smoothedGroundNormal.Normalize();
        groundNormal = smoothedGroundNormal;
    }

    private void AlignToSurface()
    {
        if (!magnetBootsOn || !isGrounded) return;

        // Check if surface changed significantly
        float normalDot = Vector3.Dot(lastGroundNormal, smoothedGroundNormal);
        if (normalDot < surfaceChangeThreshold)
        {
            // Smooth camera reset instead of instant
            float resetSpeed = 5f;
            cameraYaw = Mathf.Lerp(cameraYaw, 0f, Time.fixedDeltaTime * resetSpeed);
            cameraPitch = Mathf.Lerp(cameraPitch, 0f, Time.fixedDeltaTime * resetSpeed);

            lastGroundNormal = smoothedGroundNormal;
        }

        // Calculate target rotation using smoothed normal
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, smoothedGroundNormal) * transform.rotation;

        // Smoothly rotate toward target
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.fixedDeltaTime * rotationAlignmentSpeed
        );
    }
}