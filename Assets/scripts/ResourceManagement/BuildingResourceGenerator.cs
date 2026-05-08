using UnityEngine;

public class BuildingResourceGenerator : MonoBehaviour
{
    [Header("Configuration")]
    public ResourceType resourceType = ResourceType.Food;
    public float generationRate = 1f; // Resources per minute
    public float maxCapacity = 100f;

    [Header("Animation")]
    public GameObject deliveryAgentPrefab;

    [Header("Current State")]
    public float currentAmount = 0f;
    public string assignedDistrictID = "global";

    private GameObject fullIndicator;

    private void Start()
    {
        InitializeDistrict();
        fullIndicator = transform.Find("Full")?.gameObject;
        UpdateFullIndicator();
    }

    private void InitializeDistrict()
    {
        // 🔹 NEW — Prefer RoadConnector district
        RoadConnector connector = GetComponent<RoadConnector>();
        if (connector != null && connector.isConnected && connector.districtID != "none")
        {
            assignedDistrictID = connector.districtID;
            DevTools.Log($"[ResourceGenerator] {gameObject.name} assigned to district {assignedDistrictID} via RoadConnector.");
            return;
        }

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
        GenerateResource();
        UpdateFullIndicator();
    }

    private void GenerateResource()
    {
        if (currentAmount < maxCapacity)
        {
            float multiplier = 1.0f;
            if (PlayerData.Instance != null)
            {
                multiplier = PlayerData.Instance.GetProductionMultiplier(resourceType);
            }

            currentAmount += (generationRate / 60f) * multiplier * Time.deltaTime;
            
            // Clamp to max capacity
            if (currentAmount > maxCapacity)
            {
                currentAmount = maxCapacity;
            }
        }
    }

    /// <summary>
    /// Harvests the generated resources and transfers them to the Player Inventory.
    /// </summary>
    public void Harvest()
    {
        // 🔹 NEW — Check for road connection
        RoadConnector connector = GetComponent<RoadConnector>();
        if (connector != null && !connector.isConnected)
        {
            DevTools.LogWarning($"[Harvest] {gameObject.name} is not connected to a road! Harvest blocked.");
            return;
        }

        if (currentAmount <= 0) return;

        int amountToTransfer = Mathf.FloorToInt(currentAmount);
        
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.AddResource(resourceType, amountToTransfer, assignedDistrictID);
            currentAmount -= amountToTransfer; // Subtract harvested amount, keeping any fractional leftover
            
            // 🔹 TRIGGER DELIVERY ANIMATION
            if (deliveryAgentPrefab != null && connector != null)
            {
                DeliverAnim.StartDelivery(
                    deliveryAgentPrefab, 
                    transform.position, 
                    connector.gridCoordinate, 
                    assignedDistrictID
                );
            }

            DevTools.Log($"Harvested {amountToTransfer} {resourceType} from {gameObject.name} to district {assignedDistrictID}");
        }
        else
        {
            DevTools.LogError("PlayerData instance not found! Cannot harvest resources.");
        }
    }

    private void UpdateFullIndicator()
    {
        if (fullIndicator != null)
        {
            fullIndicator.SetActive(currentAmount >= maxCapacity);
        }
    }
}
