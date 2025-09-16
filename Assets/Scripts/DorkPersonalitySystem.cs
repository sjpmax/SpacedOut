using UnityEngine;
using System.Collections.Generic;

public class DorkPersonalitySystem : MonoBehaviour
{
    public static DorkPersonalitySystem Instance;

    [Header("Personality Settings")]
    public float idleCommentInterval = 30f;
    public float sarcasticMoodChance = 0.7f;
    public int maxRepeatsBeforeVariation = 2;

    private Dictionary<string, int> messageRepeatCount = new Dictionary<string, int>();
    private float lastIdleComment = 0f;
    private List<string> recentMessages = new List<string>();

    public enum DorkMood
    {
        Sarcastic,
        Encouraging,
        Technical,
        Mysterious,
        Panicked
    }

    private DorkMood currentMood = DorkMood.Sarcastic;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Random idle comments
        if (Time.time - lastIdleComment > idleCommentInterval)
        {
            if (Random.value < 0.3f) // 30% chance every interval
            {
                ShowIdleComment();
                lastIdleComment = Time.time;
            }
        }
    }

    public void ShowContextualMessage(string baseKey, DorkMood forcedMood = DorkMood.Sarcastic)
    {
        currentMood = forcedMood;

        // Check if we've said this too many times
        if (messageRepeatCount.ContainsKey(baseKey) &&
            messageRepeatCount[baseKey] >= maxRepeatsBeforeVariation)
        {
            ShowVariationMessage(baseKey);
            return;
        }

        // Track message frequency
        if (!messageRepeatCount.ContainsKey(baseKey))
            messageRepeatCount[baseKey] = 0;
        messageRepeatCount[baseKey]++;

        // Show the message with mood modifier
        string messageKey = GetMoodVariant(baseKey);
        DialogueManager.Instance.ShowDorkMessage(messageKey);
    }

    string GetMoodVariant(string baseKey)
    {
        string suffix = currentMood switch
        {
            DorkMood.Sarcastic => "_sarcastic",
            DorkMood.Encouraging => "_encouraging",
            DorkMood.Technical => "_technical",
            DorkMood.Mysterious => "_mysterious",
            DorkMood.Panicked => "_panicked",
            _ => ""
        };

        // Check if variant exists, fallback to base
        string variantKey = baseKey + suffix;
        if (LocalizationManager.Instance.GetText(variantKey) != "Localized text not found")
        {
            return variantKey;
        }
        return baseKey;
    }

    void ShowVariationMessage(string baseKey)
    {
        // Cycle through different moods for variety
        DorkMood[] moods = { DorkMood.Encouraging, DorkMood.Technical, DorkMood.Mysterious };
        DorkMood newMood = moods[Random.Range(0, moods.Length)];

        ShowContextualMessage(baseKey + "_alt", newMood);
        messageRepeatCount[baseKey] = 0; // Reset counter
    }

    void ShowIdleComment()
    {
        string[] idleKeys = { "dork_idle_1", "dork_idle_2", "dork_idle_3" };
        string randomKey = idleKeys[Random.Range(0, idleKeys.Length)];

        DialogueManager.Instance.ShowDorkMessage(randomKey, 2f);
    }

    // Called by game events
    public void OnPlayerAction(string action)
    {
        switch (action)
        {
            case "collected_ice":
                if (Random.value < 0.3f) // Sometimes comment
                    ShowContextualMessage("ice_collected_encouragement");
                break;

            case "low_oxygen":
                currentMood = DorkMood.Panicked;
                ShowContextualMessage("warning_low_oxygen");
                break;

            case "discovered_debris":
                currentMood = DorkMood.Mysterious;
                ShowContextualMessage("mystery_hint_1");
                break;
        }
    }

    // Dynamic mood changes based on game state
    public void UpdateMoodBasedOnGameState()
    {
        float oxygenPercent = ResourceManager.Instance.currentOxygen / ResourceManager.Instance.maxOxygen;

        if (oxygenPercent < 0.3f)
            currentMood = DorkMood.Panicked;
        else if (oxygenPercent > 0.8f)
            currentMood = Random.value < sarcasticMoodChance ? DorkMood.Sarcastic : DorkMood.Encouraging;
    }
}