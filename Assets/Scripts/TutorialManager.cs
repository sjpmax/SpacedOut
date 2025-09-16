using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Tutorial State")]
    public TutorialPhase currentPhase = TutorialPhase.Awakening;
    public int waterGlobsCollected = 0;
    public int waterGlobsNeeded = 3;

    private bool hasShownLowOxygenWarning = false;

    public enum TutorialPhase
    {
        Awakening,
        LearningWater,
        Completed
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartTutorial();
    }

    void StartTutorial()
    {
        DialogueManager.Instance.ShowDorkMessage("tutorial_awakening", 4f);
        Invoke("ShowOxygenExplanation", 5f);
    }

    void ShowOxygenExplanation()
    {
        DialogueManager.Instance.ShowDorkMessage("tutorial_oxygen_explanation", 6f);
        Invoke("ShowWaterInstructions", 7f);
    }

    void ShowWaterInstructions()
    {
        DialogueManager.Instance.ShowDorkMessage("tutorial_water_instructions", 4f);
        currentPhase = TutorialPhase.LearningWater;
    }

    public void OnWaterCollected()
    {
        waterGlobsCollected++;

        if (currentPhase == TutorialPhase.LearningWater)
        {
            if (waterGlobsCollected == 1)
                DialogueManager.Instance.ShowDorkMessage("tutorial_first_water", 3f);
            else if (waterGlobsCollected == 2)
                DialogueManager.Instance.ShowDorkMessage("tutorial_second_water", 3f);
            else if (waterGlobsCollected >= waterGlobsNeeded)
                CompleteTutorial();
        }
    }

    void CompleteTutorial()
    {
        currentPhase = TutorialPhase.Completed;
        DialogueManager.Instance.ShowDorkMessage("tutorial_complete", 5f);
    }

    public void OnLowOxygen(float remaining)
    {
        if (remaining <= 60 && !hasShownLowOxygenWarning)
        {
            DialogueManager.Instance.ShowDorkMessage("warning_low_oxygen", 3f);
            hasShownLowOxygenWarning = true;
        }
        else if (remaining <= 30)
        {
            DialogueManager.Instance.ShowDorkMessage("warning_critical_oxygen", 2f);
        }
    }
}