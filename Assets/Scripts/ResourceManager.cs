using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance; // Add singleton

    [Header("Survival")]
    public float maxOxygen = 120f;
    public float currentOxygen;
    public float oxygenDrainRate = 1f;

    [Header("UI References")]
    public Slider oxygenBar;
    public TextMeshProUGUI oxygenText;

    void Awake()
    {
        Instance = this; // Set up singleton
    }

    void Start()
    {
        currentOxygen = maxOxygen; // Initialize oxygen!
    }

    void Update()
    {
        currentOxygen -= oxygenDrainRate * Time.deltaTime;

        // Check tutorial state and warn about oxygen
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnLowOxygen(currentOxygen);

        UpdateUI(); // Add this line!

        if (currentOxygen <= 0)
            GameOver();
    }

    public void AddOxygen(float amount)
    {
        currentOxygen += amount;
        currentOxygen = Mathf.Min(currentOxygen, maxOxygen);
    }

    void UpdateUI()
    {
        if (oxygenBar != null)
            oxygenBar.value = currentOxygen / maxOxygen;

        if (oxygenText != null)
        {
            string newText = $"{currentOxygen:F0}s";
            Debug.Log($"UpdateUI: currentOxygen={currentOxygen}, setting text to '{newText}', current display='{oxygenText.text}'");
            oxygenText.text = newText;
        }
    }

    void GameOver()
    {
        DialogueManager.Instance.ShowDorkMessage("game_over");
        Time.timeScale = 0;
    }
}