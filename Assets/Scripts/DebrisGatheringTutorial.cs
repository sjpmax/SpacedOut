using UnityEngine;

public class DebrisGatheringTutorial : MonoBehaviour
{
    [Header("Tutorial Progress")]
    public int iceChunksCollected = 0;
    public int iceChunksNeeded = 3;

    private bool hasShownLowOxygenWarning = false;
    private bool tutorialActive = false;

    public void StartTutorial()
    {
        tutorialActive = true;
        DialogueManager.Instance.ShowDorkMessage("tutorial_oxygen_explanation", 15f);
        Invoke("ShowIceInstructions", 15f);
    }

    void ShowIceInstructions()
    {
        DialogueManager.Instance.ShowDorkMessage("tutorial_ice_instructions", 15f);
    }

    public void OnIceCollected()
    {
        if (!tutorialActive) return;

        iceChunksCollected++;

        if (iceChunksCollected == 1)
            DialogueManager.Instance.ShowDorkMessage("tutorial_first_ice", 3f);
        else if (iceChunksCollected == 2)
            DialogueManager.Instance.ShowDorkMessage("tutorial_second_ice", 3f);
        else if (iceChunksCollected >= iceChunksNeeded)
            CompleteTutorial();
    }

    void CompleteTutorial()
    {
        tutorialActive = false;
        DialogueManager.Instance.ShowDorkMessage("tutorial_debris_gathering_complete", 5f);

        // Notify main tutorial manager
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnDebrisGatheringComplete();
    }

    public void OnLowOxygen(float remaining)
    {
        if (!tutorialActive) return;

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