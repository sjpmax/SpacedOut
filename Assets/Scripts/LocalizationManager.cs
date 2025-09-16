using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocalizationData
{
    public LocalizationItem[] items;
}

[System.Serializable]
public class LocalizationItem
{
    public string key;
    public string value;
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    private Dictionary<string, string> localizedText;
    private string missingTextString = "Localized text not found";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        LoadLocalizedText("en"); // Default to English
    }

    public void LoadLocalizedText(string languageCode)
    {
        localizedText = new Dictionary<string, string>();
        string filePath = $"Text/localizedText_{languageCode}";

        TextAsset dataAsText = Resources.Load<TextAsset>(filePath);

        if (dataAsText != null)
        {
            LocalizationData localizationData = JsonUtility.FromJson<LocalizationData>(dataAsText.text);

            for (int i = 0; i < localizationData.items.Length; i++)
            {
                localizedText.Add(localizationData.items[i].key, localizationData.items[i].value);
            }
        }
    }

    public string GetText(string key)
    {
        if (localizedText != null && localizedText.ContainsKey(key))
            return localizedText[key];
        else
            return missingTextString;
    }
}