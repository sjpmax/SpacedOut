using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public AudioSource typewriterSound;

    [Header("Typewriter Settings")]
    public float defaultTypeSpeed = 0.05f;
    public float fastTypeSpeed = 0.02f;
    public float slowTypeSpeed = 0.1f;
    public bool skipOnClick = true;

    [Header("Audio Settings")]
    public AudioClip[] typewriterClips;
    public float audioVolume = 0.5f;

    private float messageTimer = 0f;
    private Coroutine currentTypewriter;
    private bool isTyping = false;
    private string currentFullText = "";
    private InputSystem_Actions inputActions;

    // Rich text effect patterns
    private Dictionary<string, System.Func<string, string>> textEffects;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTextEffects();

            // Initialize input actions
            inputActions = new InputSystem_Actions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            // Check if SkipDialogue action exists, fallback to right mouse if not
            try
            {
                inputActions.Player.SkipDialogue.performed += OnSkipDialogue;
            }
            catch
            {
                // Fallback: use Mouse class directly for right mouse button
                Debug.LogWarning("SkipDialogue action not found in InputActions. Add it for better integration.");
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

        // Fallback input handling if SkipDialogue action doesn't exist
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

    void InitializeTextEffects()
    {
        textEffects = new Dictionary<string, System.Func<string, string>>
        {
            {"[SARCASM]", text => $"<i><color=#FF6B6B>{text}</color></i>"},
            {"[EXCITEMENT]", text => $"<color=#FFD93D><size=120%>{text}</size></color>"},
            {"[WHISPER]", text => $"<alpha=#80><size=80%>{text}</size></alpha>"},
            {"[SYSTEM]", text => $"<color=#00FF00><font=\"LiberationSans SDF\">{text}</font></color>"},
            {"[WARNING]", text => $"<color=#FF0000><b>{text}</b></color>"},
            {"[LOADING]", text => $"<color=#888888>{text}...</color>"},
            {"[MEMORY]", text => $"<color=#9B59B6><i>{text}</i></color>"},
            {"[HIGHLIGHT]", text => $"<mark=#FFFF00AA>{text}</mark>"}
        };
    }

    public void ShowDorkMessage(string messageKey, float duration = 3f)
    {
        string localizedText = LocalizationManager.Instance.GetText(messageKey);
        ShowMessage($"DORK: {localizedText}", duration);
    }

    public void ShowMessage(string message, float duration = 3f, float typeSpeed = -1f)
    {
        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
        }

        string processedText = ProcessTextEffects(message);
        currentFullText = processedText;
        messageTimer = duration;

        float speed = typeSpeed > 0 ? typeSpeed : defaultTypeSpeed;
        currentTypewriter = StartCoroutine(TypewriterEffect(processedText, speed));
    }

    string ProcessTextEffects(string text)
    {
        string processed = text;

        // Process custom markup tags
        foreach (var effect in textEffects)
        {
            string openTag = Regex.Escape(effect.Key);
            string closeTag = Regex.Escape(effect.Key.Replace("[", "[/"));
            string pattern = $@"{openTag}(.*?){closeTag}";
            processed = Regex.Replace(processed, pattern, match => effect.Value(match.Groups[1].Value));
        }

        // Process speed modifiers - use custom format to avoid TextMeshPro conflicts
        processed = processed.Replace("[FAST]", "{{SPEED:fast}}");
        processed = processed.Replace("[/FAST]", "{{SPEED:normal}}");
        processed = processed.Replace("[SLOW]", "{{SPEED:slow}}");
        processed = processed.Replace("[/SLOW]", "{{SPEED:normal}}");

        // Process pauses - use custom format to avoid TextMeshPro conflicts
        processed = processed.Replace("[PAUSE]", "{{PAUSE:1}}");
        processed = processed.Replace("[PAUSE_LONG]", "{{PAUSE:2}}");

        return processed;
    }

    IEnumerator TypewriterEffect(string fullText, float baseSpeed)
    {
        isTyping = true;
        dialogueText.text = "";

        string visibleText = "";
        float currentSpeed = baseSpeed;

        // Strip rich text tags for character counting but keep them for display
        string plainText = Regex.Replace(fullText, "<.*?>", "");
        int plainTextIndex = 0;

        for (int i = 0; i < fullText.Length; i++)
        {
            char currentChar = fullText[i];

            // Handle our custom pause tags
            if (currentChar == '{' && i + 1 < fullText.Length && fullText[i + 1] == '{')
            {
                // Find the end of our custom tag
                int tagEnd = fullText.IndexOf("}}", i);
                if (tagEnd != -1)
                {
                    string customTag = fullText.Substring(i + 2, tagEnd - i - 2); // Skip {{ and }}

                    // Process pause tags
                    if (customTag.StartsWith("PAUSE:"))
                    {
                        float pauseTime = 1f;
                        var match = Regex.Match(customTag, @"PAUSE:(\d+\.?\d*)");
                        if (match.Success)
                            float.TryParse(match.Groups[1].Value, out pauseTime);

                        dialogueText.text = visibleText;
                        yield return new WaitForSeconds(pauseTime);
                    }

                    // Process speed tags
                    if (customTag.StartsWith("SPEED:"))
                    {
                        string speedValue = customTag.Substring(6); // Remove "SPEED:"
                        currentSpeed = speedValue switch
                        {
                            "fast" => fastTypeSpeed,
                            "slow" => slowTypeSpeed,
                            "normal" => baseSpeed,
                            _ => baseSpeed
                        };
                    }

                    i = tagEnd + 1; // Skip to end of custom tag
                    continue;
                }
            }

            // Handle rich text tags
            if (currentChar == '<')
            {
                // Find the end of the tag
                int tagEnd = fullText.IndexOf('>', i);
                if (tagEnd != -1)
                {
                    string tag = fullText.Substring(i, tagEnd - i + 1);
                    visibleText += tag;

                    // Process speed modification tags
                    if (tag.Contains("speed=fast"))
                        currentSpeed = fastTypeSpeed;
                    else if (tag.Contains("speed=slow"))
                        currentSpeed = slowTypeSpeed;
                    else if (tag.Contains("speed=normal"))
                        currentSpeed = baseSpeed;

                    // Process pause tags - look for our custom format
                    if (tag.Contains("PAUSE:"))
                    {
                        float pauseTime = 1f;
                        var match = Regex.Match(tag, @"PAUSE:(\d+\.?\d*)");
                        if (match.Success)
                            float.TryParse(match.Groups[1].Value, out pauseTime);

                        dialogueText.text = visibleText;
                        yield return new WaitForSeconds(pauseTime);
                    }

                    i = tagEnd; // Skip to end of tag
                    continue;
                }
            }

            visibleText += currentChar;
            dialogueText.text = visibleText;

            // Only play sound and wait for actual characters (not tags)
            if (!char.IsWhiteSpace(currentChar))
            {
                PlayTypewriterSound();
                plainTextIndex++;
            }

            yield return new WaitForSeconds(currentSpeed);
        }

        isTyping = false;
    }

    void PlayTypewriterSound()
    {
        if (typewriterSound != null && typewriterClips != null && typewriterClips.Length > 0)
        {
            AudioClip randomClip = typewriterClips[Random.Range(0, typewriterClips.Length)];
            typewriterSound.PlayOneShot(randomClip, audioVolume);
        }
    }

    void SkipTypewriter()
    {
        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
            // Clean up custom tags when skipping
            string cleanedText = CleanCustomTags(currentFullText);
            dialogueText.text = cleanedText;
            isTyping = false;
        }
    }

    string CleanCustomTags(string text)
    {
        // Remove all custom tags like {{PAUSE:1}}, {{SPEED:fast}}, etc.
        string cleaned = Regex.Replace(text, @"\{\{[^}]*\}\}", "");
        return cleaned;
    }

    public void ClearMessage()
    {
        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
        }

        if (dialogueText != null)
            dialogueText.text = "";

        isTyping = false;
        currentFullText = "";
    }

    // Public method to add custom text effects at runtime
    public void AddTextEffect(string tag, System.Func<string, string> effect)
    {
        textEffects[tag] = effect;
    }
}