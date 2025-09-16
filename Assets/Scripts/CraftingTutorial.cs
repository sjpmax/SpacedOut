
using UnityEngine;

public class CraftingTutorial : MonoBehaviour
{
	public void StartTutorial()
	{
		DialogueManager.Instance.ShowDorkMessage("tutorial_welding_start", 4f);
		// Teach welding mechanics
	}
}