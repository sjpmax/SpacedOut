using UnityEngine;


public class DebrisCollector : MonoBehaviour
{
    [Header("Collection Settings")]
    public float collectionRange = 2f;
    public LayerMask debrisLayer = -1; // What layers to collect from

    [Header("Audio/Effects")]
    public AudioSource collectSound;
    public ParticleSystem collectEffect;

    void Update()
    {
        CheckForDebris();
    }

    void CheckForDebris()
    {
        Collider[] nearbyDebris = Physics.OverlapSphere(transform.position, collectionRange, debrisLayer);

        foreach (Collider debris in nearbyDebris)
        {
            if (debris.CompareTag("Ice") || debris.CompareTag("Metal"))
            {
                CollectDebris(debris.gameObject);
                break; // Only collect one per frame
            }
        }
    }

    void CollectDebris(GameObject debris)
    {
        string debrisType = debris.tag;

        switch (debrisType)
        {
            case "Ice":
                CollectIce(debris);
                break;
            case "Metal":
                CollectMetal(debris);
                break;
        }

        // Play collection effects
        PlayCollectionEffects();

        // Destroy the debris
        Destroy(debris);
    }

    void CollectIce(GameObject iceDebris)
    {
        // Get ice properties
        IceChunk iceChunk = iceDebris.GetComponent<IceChunk>();
        float oxygenGained = 30f; // Default
        string sizeName = "medium";

        if (iceChunk != null)
        {
            oxygenGained = iceChunk.GetOxygenValue();
            sizeName = iceChunk.GetSizeName();
        }

        // Add oxygen to resource manager
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddOxygen(oxygenGained);
        }

        // Notify tutorial system
        if (TutorialManager.Instance?.debrisTutorial != null)
        {
            TutorialManager.Instance.debrisTutorial.OnIceCollected();
        }

        // Show feedback message ONLY if not in awakening phase
        if (DialogueManager.Instance != null && TutorialManager.Instance != null)
        {
            if (!TutorialManager.Instance.IsInAwakening)
            {
                DialogueManager.Instance.ShowDorkMessage($"ice_collected_{sizeName}", 5f);
            }
        }

        Debug.Log($"Collected {sizeName} ice chunk! +{oxygenGained} seconds oxygen");
    }

    void CollectMetal(GameObject metalDebris)
    {
        // Handle metal collection (for crafting)
        // Add to inventory when inventory system exists

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDorkMessage("metal_collected", 2f);
        }

        Debug.Log("Collected metal scrap!");
    }

    void PlayCollectionEffects()
    {
        // Play sound effect
        if (collectSound != null)
        {
            collectSound.Play();
        }

        // Play particle effect
        if (collectEffect != null)
        {
            collectEffect.Play();
        }
    }

    // Visual debug in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collectionRange);
    }
}