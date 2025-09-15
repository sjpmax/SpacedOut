using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    public float speed = 20f; // mph converted to units/second

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}