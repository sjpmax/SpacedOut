using UnityEngine;
using UnityEngine.InputSystem;

public class DebrisSpawner : MonoBehaviour
{
    public GameObject debrisPrefab;
    public float spawnDistance = 50f;
    public float spawnInterval = 2f;
    public float horizontalRange = 15f; // How far left/right
    public float verticalRange = 10f;   // How far up/down

    private Transform platform;

    void Start()
    {
        platform = FindObjectOfType<PlatformMover>().transform;
        InvokeRepeating("SpawnDebris", 1f, spawnInterval);
    }

    void SpawnDebris()
    {
        Vector3 spawnPos = platform.position + Vector3.forward * spawnDistance;

        // Scatter debris in a wide area around the platform's path
        spawnPos.x += Random.Range(-horizontalRange, horizontalRange);
        spawnPos.y += Random.Range(-verticalRange, verticalRange);
        spawnPos.z += Random.Range(-5f, 5f); // A little depth variation too

        Instantiate(debrisPrefab, spawnPos, Random.rotation);
    }
}