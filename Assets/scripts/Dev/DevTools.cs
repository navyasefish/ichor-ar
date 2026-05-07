using UnityEngine;

public class DevTools : MonoBehaviour
{
    public static DevTools Instance;

    [Header("Debug Settings")]
    public bool debugMode = false;

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
}
