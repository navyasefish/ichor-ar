using UnityEngine;
using TMPro;

public class DevTools : MonoBehaviour
{
    public static DevTools Instance;

    [Header("Debug Settings")]
    public bool debugMode = false;
    public bool anchorBasedSpawning = true;
    public bool saveState = true;
    public TextMeshProUGUI statusText;

    public void ToggleAnchorBasedSpawning()
    {
        anchorBasedSpawning = !anchorBasedSpawning;
        SetStatus($"Anchor Spawning: {(anchorBasedSpawning ? "ENABLED" : "DISABLED")}");
    }

    private void Awake()
    {
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

    /// <summary>
    /// Log a message only if debug mode is enabled.
    /// </summary>
    public static void Log(string message)
    {
        if (Instance != null && Instance.debugMode)
        {
            Debug.Log(message);
        }
    }

    /// <summary>
    /// Log a warning only if debug mode is enabled.
    /// </summary>
    public static void LogWarning(string message)
    {
        if (Instance != null && Instance.debugMode)
        {
            Debug.LogWarning(message);
        }
    }

    /// <summary>
    /// Log an error only if debug mode is enabled.
    /// </summary>
    public static void LogError(string message)
    {
        if (Instance != null && Instance.debugMode)
        {
            Debug.LogError(message);
        }
    }

    public static void SetStatus(string message)
    {
        if (Instance != null && Instance.statusText != null)
        {
            Instance.statusText.text = message;
        }
        Log($"[STATUS] {message}");
    }
}
