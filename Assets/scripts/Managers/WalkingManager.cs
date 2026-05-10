using UnityEngine;

public class WalkingManager : MonoBehaviour
{
    public static WalkingManager Instance { get; private set; }

    [Header("Walker Settings")]
    public GameObject[] walkerPrefabs;
    public int max_limit = 10;
    public float minSpeed = 0.5f;
    public float maxSpeed = 1.5f;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    [Header("Status")]
    public int currentWalkerCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool CanSpawn()
    {
        return currentWalkerCount < max_limit;
    }

    public void RegisterWalker() => currentWalkerCount++;
    public void UnregisterWalker() => currentWalkerCount--;
}
