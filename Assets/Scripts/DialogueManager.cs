using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance; // Singleton pattern

    [Header("UI References")]
    public TextMeshProUGUI dialogueText;

    private float messageTimer = 0f;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0)
                ClearMessage();
        }
    }

    public void ShowDorkMessage(string messageKey, float duration = 3f)
    {
        // Get localized text from LocalizationManager
        string localizedText = LocalizationManager.Instance.GetText(messageKey);

        if (dialogueText != null)
        {
            dialogueText.text = $"DORK: {localizedText}";
            messageTimer = duration;
        }
    }

    public void ClearMessage()
    {
        if (dialogueText != null)
            dialogueText.text = "";
    }
}