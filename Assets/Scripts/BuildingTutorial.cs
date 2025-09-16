using UnityEngine;


public class BuildingTutorial : MonoBehaviour
{
    public void StartTutorial()
    {
        DialogueManager.Instance.ShowDorkMessage("tutorial_building_start", 4f);
        // Teach ship construction
    }
}