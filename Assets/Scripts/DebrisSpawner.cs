
using UnityEngine;
using UnityEngine.InputSystem;

public class DebrisSpawner : MonoBehaviour
{
    [Header("Debris Prefabs")]
    public GameObject[] IceDebrisPrefabs;  // Array of white balls of ice
    public GameObject[] metalDebrisPrefabs;  // Array of gray/orange metal

    [Header("Spawn Settings")]
    public float spawnDistance = 50f;
    public float spawnInterval = 2f;
    public float horizontalRange = 15f;
    public float verticalRange = 10f;

    [Header("Size Variation")]
    public float minScale = 0.5f;  // Smallest debris
    public float maxScale = 1.5f;  // Largest debris

    [Header("Tutorial Settings")]
    public bool tutorialMode = true; // Spawn mostly ice during tutorial

    private Transform platform;

    void Start()
    {
        platform = FindFirstObjectByType<PlatformMover>().transform;
        InvokeRepeating("SpawnDebris", 1f, spawnInterval);
    }

    void SpawnDebris()
    {
        Vector3 spawnPos = platform.position + Vector3.forward * spawnDistance;
        spawnPos.x += Random.Range(-horizontalRange, horizontalRange);
        spawnPos.y += Random.Range(-verticalRange, verticalRange);
        spawnPos.z += Random.Range(-5f, 5f);

        GameObject debrisToSpawn;

        if (tutorialMode)
        {
            // 80% ice, 20% metal during tutorial
            if (Random.value < 0.8f)
                debrisToSpawn = IceDebrisPrefabs[Random.Range(0, IceDebrisPrefabs.Length)];
            else
                debrisToSpawn = metalDebrisPrefabs[Random.Range(0, metalDebrisPrefabs.Length)];
        }
        else
        {
            // 50/50 split after tutorial
            if (Random.value < 0.5f)
                debrisToSpawn = IceDebrisPrefabs[Random.Range(0, IceDebrisPrefabs.Length)];
            else
                debrisToSpawn = metalDebrisPrefabs[Random.Range(0, metalDebrisPrefabs.Length)];
        }

        // Spawn the debris
        GameObject spawnedDebris = Instantiate(debrisToSpawn, spawnPos, Random.rotation);

        // ADD SIZE VARIATION HERE
        float randomScale = Random.Range(minScale, maxScale);
        spawnedDebris.transform.localScale = Vector3.one * randomScale;
    }

    public void EndTutorialMode()
    {
        tutorialMode = false;
    }
}

