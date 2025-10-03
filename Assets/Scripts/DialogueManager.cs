using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI dialogueText;

    [Header("Terminal Style Settings")]
    public Color normalColor = new Color(0f, 1f, 0f); // Green
    public Color warningColor = new Color(1f, 0f, 0f); // Red
    public Color systemColor = new Color(0f, 0.8f, 1f); // Cyan
    [Range(0f, 1f)]
    public float outlineWidth = 0.2f;
    public Color outlineColor = Color.black;

    [Header("Typewriter Settings")]
    public float defaultTypeSpeed = 0.03f;
    public bool skipOnClick = true;

    private InputSystem_Actions inputActions;
    private string currentFullText;
    private Coroutine currentTypewriter;
    private bool isTyping = false;
    private float messageTimer = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        inputActions = new InputSystem_Actions();

        // Set up text outline
        if (dialogueText != null)
        {
            dialogueText.outlineWidth = outlineWidth;
            dialogueText.outlineColor = outlineColor;
            dialogueText.fontStyle = FontStyles.Normal; // No bold, italic, etc.
        }
    }

    void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            // Try to hook up skip dialogue if action exists
            try
            {
                inputActions.Player.SkipDialogue.performed += OnSkipDialogue;
            }
            catch
            {
                Debug.LogWarning("SkipDialogue action not found in InputSystem_Actions.");
            }
        }
    }

    void OnDisable()
    {
        if (inputActions != null)
        {
            try
            {
                inputActions.Player.SkipDialogue.performed -= OnSkipDialogue;
            }
            catch { }
            inputActions.Disable();
        }
    }

    void Start()
    {
        if (dialogueText != null)
            dialogueText.text = "";
    }

    void Update()
    {
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0)
                ClearMessage();
        }

        // Fallback: Right-click to skip typing
        if (isTyping && skipOnClick && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            SkipTypewriter();
        }
    }

    private void OnSkipDialogue(InputAction.CallbackContext context)
    {
        if (isTyping && skipOnClick)
        {
            SkipTypewriter();
        }
    }

    // Show DORK message using localization key
    public void ShowDorkMessage(string messageKey, float duration = 3f)
    {
        string localizedText = LocalizationManager.Instance.GetText(messageKey);
        ShowMessage($"> DORK: {localizedText}", duration, MessageType.Normal);
    }

    // Show message with type (Normal, Warning, System)
    public void ShowMessage(string message, float duration = 3f, MessageType type = MessageType.Normal)
    {
        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
        }

        // Apply color based on type
        Color textColor = type switch
        {
            MessageType.Warning => warningColor,
            MessageType.System => systemColor,
            _ => normalColor
        };

        // Set the vertex color directly
        dialogueText.color = textColor;

        currentFullText = message;
        messageTimer = 0f; // Don't start timer yet

        currentTypewriter = StartCoroutine(TypewriterEffect(message, defaultTypeSpeed, duration));
    }

    IEnumerator TypewriterEffect(string text, float speed, float displayDuration)
    {
        isTyping = true;
        dialogueText.text = text;

        // CRITICAL: Force TextMeshPro to update
        dialogueText.ForceMeshUpdate();

        dialogueText.maxVisibleCharacters = 0;

        int totalChars = dialogueText.textInfo.characterCount;

        for (int i = 0; i <= totalChars; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(speed);
        }

        isTyping = false;

        // NOW start the display timer after typing is complete
        messageTimer = displayDuration;
    }

    void SkipTypewriter()
    {
        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
        }

        dialogueText.maxVisibleCharacters = 99999;
        isTyping = false;
    }

    void ClearMessage()
    {
        if (dialogueText != null)
            dialogueText.text = "";
    }

    public enum MessageType
    {
        Normal,   // Green/Amber
        Warning,  // Red
        System    // Cyan
    }
}