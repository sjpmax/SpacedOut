using UnityEngine;

public class IceChunk : MonoBehaviour
{
    [Header("Ice Properties")]
    public IceSize size = IceSize.Medium;
    public float oxygenValue = 30f; // Automatically set based on size

    public enum IceSize
    {
        Small,   // 10 seconds
        Medium,  // 30 seconds 
        Large    // 60 seconds
    }

    void Start()
    {
        // Set oxygen value based on size
        switch (size)
        {
            case IceSize.Small:
                oxygenValue = 10f;
                break;
            case IceSize.Medium:
                oxygenValue = 30f;
                break;
            case IceSize.Large:
                oxygenValue = 60f;
                break;
        }
    }

    public float GetOxygenValue()
    {
        return oxygenValue;
    }

    public string GetSizeName()
    {
        return size.ToString().ToLower();
    }
}