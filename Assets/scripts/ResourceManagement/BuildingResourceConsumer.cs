using UnityEngine;

public class BuildingResourceConsumer : MonoBehaviour
{
    [Header("Configuration")]
    public ResourceType resourceType = ResourceType.Food;
    public int consumptionAmount = 5;
    public float consumptionInterval = 60f; // Every minute

    [Header("Current State")]
    public bool hasShortage = false;
    public string assignedDistrictID = "global";

    private float nextConsumptionTime;
    private GameObject emptyIndicator;

    private void Start()
    {
        InitializeDistrict();
        emptyIndicator = transform.Find("Empty")?.gameObject;
        nextConsumptionTime = Time.time + consumptionInterval;
        UpdateEmptyIndicator();
    }

    private void InitializeDistrict()
    {
        DistrictFlag[] flags = FindObjectsOfType<DistrictFlag>();
        if (flags.Length == 0) return;

        DistrictFlag nearestFlag = null;
        float minDistance = float.MaxValue;

        foreach (var flag in flags)
        {
            float distance = Vector3.Distance(transform.position, flag.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestFlag = flag;
            }
        }

        if (nearestFlag != null)
        {
            assignedDistrictID = nearestFlag.districtID;
        }
    }

    private void Update()
    {
        if (Time.time >= nextConsumptionTime)
        {
            ConsumeResource();
            nextConsumptionTime = Time.time + consumptionInterval;
        }

        // Periodically check for real-time shortage feedback
        CheckShortageStatus();
    }

    private void ConsumeResource()
    {
        if (PlayerData.Instance != null)
        {
            bool success = PlayerData.Instance.SpendResource(resourceType, consumptionAmount, assignedDistrictID);
            hasShortage = !success;
            
            if (success)
            {
                DevTools.Log($"{gameObject.name} in district {assignedDistrictID} consumed {consumptionAmount} {resourceType}");
            }
            else
            {
                DevTools.LogWarning($"{gameObject.name} in district {assignedDistrictID} failed to consume {resourceType} - Shortage!");
            }
        }
    }

    private void CheckShortageStatus()
    {
        if (PlayerData.Instance != null)
        {
            // Update shortage status if we don't have enough for the next cycle
            bool canAfford = PlayerData.Instance.HasEnough(resourceType, consumptionAmount, assignedDistrictID);
            
            // We show shortage if we either failed the last consumption OR we can't afford the next one
            bool shouldShowEmpty = hasShortage || !canAfford;
            
            if (emptyIndicator != null && emptyIndicator.activeSelf != shouldShowEmpty)
            {
                emptyIndicator.SetActive(shouldShowEmpty);
            }
        }
    }

    private void UpdateEmptyIndicator()
    {
        if (emptyIndicator != null)
        {
            emptyIndicator.SetActive(hasShortage);
        }
    }
}
