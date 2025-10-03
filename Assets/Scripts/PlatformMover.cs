using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    public float speed = 20f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // ← KEY!
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + Vector3.forward * speed * Time.fixedDeltaTime);
    }
}