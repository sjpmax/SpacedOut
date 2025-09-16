using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Tutorial Components")]
    public DebrisGatheringTutorial debrisTutorial;
    public CraftingTutorial craftingTutorial;
    public BuildingTutorial buildingTutorial;

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
        StartAwakeningSequence();
    }

    void StartAwakeningSequence()
    {

        Debug.Log("Awakening");
        DialogueManager.Instance.ShowDorkMessage("tutorial_awakening", 4f);
        Debug.Log("Awakening sentence over");
        Invoke("StartDebrisGatheringTutorial", 5f);
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