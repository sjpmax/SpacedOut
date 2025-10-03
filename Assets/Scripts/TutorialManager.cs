using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Tutorial Components")]
    public DebrisGatheringTutorial debrisTutorial;
    public CraftingTutorial craftingTutorial;
    public BuildingTutorial buildingTutorial;
    public bool IsInAwakening = true;

    [Header("Timing Settings")]
    public float initialDelay = 2f; // Wait 2 seconds before first message

    public enum TutorialPhase
    {
        Awakening,
        DebrisGathering,
        BasicCrafting,
        ShipBuilding,
        Completed
    }

    public TutorialPhase currentPhase = TutorialPhase.Awakening;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(AwakeningSequence());
    }

    IEnumerator AwakeningSequence()
    {
        Debug.Log("Starting awakening sequence...");

        // Wait for initial delay to ensure everything is loaded
        yield return new WaitForSeconds(initialDelay);

        // Check if DialogueManager is ready
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager.Instance is NULL!");
            yield break;
        }

        // Show all awakening messages in sequence
        Debug.Log("Message 1: Oh hey, you're awake!");
        DialogueManager.Instance.ShowDorkMessage("tutorial_awakening", 10f);
        yield return new WaitForSeconds(9f); // Wait for typing + display

        Debug.Log("Message 2: Something incredible!");
        DialogueManager.Instance.ShowDorkMessage("tutorial_awakening_2", 10f);
        yield return new WaitForSeconds(9f);

        Debug.Log("Message 3: Meat Popsicle");
        DialogueManager.Instance.ShowDorkMessage("tutorial_awakening_3", 10f);
        yield return new WaitForSeconds(8f);

        Debug.Log("Message 4: Blue chunks");
        DialogueManager.Instance.ShowDorkMessage("tutorial_awakening_4", 10f);
        yield return new WaitForSeconds(9f);
        
        Debug.Log("Message 5: Magnet Boots");
        DialogueManager.Instance.ShowDorkMessage("tutorial_awakening_5", 10f);
        yield return new WaitForSeconds(9f);

        IsInAwakening = false;

        Debug.Log("Starting debris gathering tutorial");
        StartDebrisGatheringTutorial();
    }

    void StartDebrisGatheringTutorial()
    {
        currentPhase = TutorialPhase.DebrisGathering;
        if (debrisTutorial != null)
            debrisTutorial.StartTutorial();
    }

    public void OnDebrisGatheringComplete()
    {
        currentPhase = TutorialPhase.BasicCrafting;
        DialogueManager.Instance.ShowDorkMessage("tutorial_debris_complete", 4f);
        // Start next tutorial phase
    }

    public void OnLowOxygen(float remaining)
    {
        if (currentPhase == TutorialPhase.DebrisGathering && debrisTutorial != null)
        {
            debrisTutorial.OnLowOxygen(remaining);
        }
    }
}