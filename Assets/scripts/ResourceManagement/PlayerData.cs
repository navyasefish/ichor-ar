using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    [Header("Resources")]
    public int food;
    public int stone;
    public int gold;
    public int population;
    public int ichor;

    [Header("God Favors (0-10)")]
    public int favorZeus;
    public int favorAres;
    public int favorArtemis;
    public int favorDemeter;
    public int favorPoseidon;
    public int favorHephaestus;

    private System.Collections.Generic.Dictionary<string, DistrictInventorySaveData> districtInventories = new System.Collections.Generic.Dictionary<string, DistrictInventorySaveData>();
    private string lastDistrictID = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlayerData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        SyncToActiveDistrict();
    }

    private void SyncToActiveDistrict()
    {
        if (DistrictManager.Instance == null) return;

        string currentID = DistrictManager.Instance.currentDistrictID;
        if (currentID != lastDistrictID)
        {
            // Save current fields to the old district inventory before switching
            if (!string.IsNullOrEmpty(lastDistrictID))
            {
                UpdateInventoryData(lastDistrictID);
            }

            // Load fields from the new district inventory
            ApplyInventoryData(currentID);
            lastDistrictID = currentID;
        }
    }

    private void UpdateInventoryData(string districtID)
    {
        if (!districtInventories.ContainsKey(districtID))
        {
            districtInventories[districtID] = new DistrictInventorySaveData { districtID = districtID };
        }
        var inv = districtInventories[districtID];
        inv.food = food;
        inv.stone = stone;
        inv.gold = gold;
        inv.population = population;
        inv.ichor = ichor;
    }

    private void ApplyInventoryData(string districtID)
    {
        if (!districtInventories.ContainsKey(districtID))
        {
            districtInventories[districtID] = new DistrictInventorySaveData { districtID = districtID };
        }
        var inv = districtInventories[districtID];
        food = inv.food;
        stone = inv.stone;
        gold = inv.gold;
        population = inv.population;
        ichor = inv.ichor;
    }

    public void AddResource(ResourceType type, int amount)
    {
        AddResource(type, amount, lastDistrictID);
    }

    public void AddResource(ResourceType type, int amount, string districtID)
    {
        if (string.IsNullOrEmpty(districtID)) districtID = "global";

        if (districtID == lastDistrictID)
        {
            switch (type)
            {
                case ResourceType.Food: food += amount; break;
                case ResourceType.Stone: stone += amount; break;
                case ResourceType.Gold: gold += amount; break;
                case ResourceType.Population: population += amount; break;
                case ResourceType.Ichor: ichor += amount; break;
            }
        }
        else
        {
            if (!districtInventories.ContainsKey(districtID))
                districtInventories[districtID] = new DistrictInventorySaveData { districtID = districtID };
            
            var inv = districtInventories[districtID];
            switch (type)
            {
                case ResourceType.Food: inv.food += amount; break;
                case ResourceType.Stone: inv.stone += amount; break;
                case ResourceType.Gold: inv.gold += amount; break;
                case ResourceType.Population: inv.population += amount; break;
                case ResourceType.Ichor: inv.ichor += amount; break;
            }
        }
        SavePlayerData();
    }

    public bool HasEnough(ResourceType type, int amount)
    {
        return HasEnough(type, amount, lastDistrictID);
    }

    public bool HasEnough(ResourceType type, int amount, string districtID)
    {
        if (string.IsNullOrEmpty(districtID)) districtID = "global";

        if (districtID == lastDistrictID)
        {
            switch (type)
            {
                case ResourceType.Food: return food >= amount;
                case ResourceType.Stone: return stone >= amount;
                case ResourceType.Gold: return gold >= amount;
                case ResourceType.Population: return population >= amount;
                case ResourceType.Ichor: return ichor >= amount;
                default: return false;
            }
        }
        else
        {
            if (!districtInventories.ContainsKey(districtID)) return false;
            var inv = districtInventories[districtID];
            switch (type)
            {
                case ResourceType.Food: return inv.food >= amount;
                case ResourceType.Stone: return inv.stone >= amount;
                case ResourceType.Gold: return inv.gold >= amount;
                case ResourceType.Population: return inv.population >= amount;
                case ResourceType.Ichor: return inv.ichor >= amount;
                default: return false;
            }
        }
    }

    public bool SpendResource(ResourceType type, int amount)
    {
        return SpendResource(type, amount, lastDistrictID);
    }

    public bool SpendResource(ResourceType type, int amount, string districtID)
    {
        if (!HasEnough(type, amount, districtID)) return false;

        if (districtID == lastDistrictID)
        {
            switch (type)
            {
                case ResourceType.Food: food -= amount; break;
                case ResourceType.Stone: stone -= amount; break;
                case ResourceType.Gold: gold -= amount; break;
                case ResourceType.Population: population -= amount; break;
                case ResourceType.Ichor: ichor -= amount; break;
            }
        }
        else
        {
            var inv = districtInventories[districtID];
            switch (type)
            {
                case ResourceType.Food: inv.food -= amount; break;
                case ResourceType.Stone: inv.stone -= amount; break;
                case ResourceType.Gold: inv.gold -= amount; break;
                case ResourceType.Population: inv.population -= amount; break;
                case ResourceType.Ichor: inv.ichor -= amount; break;
            }
        }
        SavePlayerData();
        return true;
    }

    public void AddFavor(string godName, int amount)
    {
        switch (godName.ToLower())
        {
            case "zeus": favorZeus = Mathf.Clamp(favorZeus + amount, 0, 10); break;
            case "ares": favorAres = Mathf.Clamp(favorAres + amount, 0, 10); break;
            case "artemis": favorArtemis = Mathf.Clamp(favorArtemis + amount, 0, 10); break;
            case "demeter": favorDemeter = Mathf.Clamp(favorDemeter + amount, 0, 10); break;
            case "poseidon": favorPoseidon = Mathf.Clamp(favorPoseidon + amount, 0, 10); break;
            case "hephaestus": favorHephaestus = Mathf.Clamp(favorHephaestus + amount, 0, 10); break;
            default: DevTools.LogWarning($"Unknown god name: {godName}"); return;
        }
        SavePlayerData();
    }

    public void SavePlayerData()
    {
        if (SaveSystem.Instance == null) return;

        // Ensure current active district is updated in the dictionary before saving
        if (!string.IsNullOrEmpty(lastDistrictID))
        {
            UpdateInventoryData(lastDistrictID);
        }

        SaveData data = SaveSystem.Instance.LoadData();
        
        // Save god favors (global)
        data.player.favorZeus = favorZeus;
        data.player.favorAres = favorAres;
        data.player.favorArtemis = favorArtemis;
        data.player.favorDemeter = favorDemeter;
        data.player.favorPoseidon = favorPoseidon;
        data.player.favorHephaestus = favorHephaestus;

        // Save all district inventories
        data.districtInventories.Clear();
        foreach (var inv in districtInventories.Values)
        {
            data.districtInventories.Add(inv);
        }

        SaveSystem.Instance.SaveAllData(data);
    }

    public void LoadPlayerData()
    {
        if (SaveSystem.Instance == null) return;

        SaveData data = SaveSystem.Instance.LoadData();
        
        // Load god favors (global)
        favorZeus = data.player.favorZeus;
        favorAres = data.player.favorAres;
        favorArtemis = data.player.favorArtemis;
        favorDemeter = data.player.favorDemeter;
        favorPoseidon = data.player.favorPoseidon;
        favorHephaestus = data.player.favorHephaestus;

        // Load all district inventories
        districtInventories.Clear();
        foreach (var inv in data.districtInventories)
        {
            districtInventories[inv.districtID] = inv;
        }

        // Apply current district data if possible
        if (DistrictManager.Instance != null)
        {
            lastDistrictID = DistrictManager.Instance.currentDistrictID;
            ApplyInventoryData(lastDistrictID);
        }
        else
        {
            // Default to global if DistrictManager not yet ready
            lastDistrictID = "global";
            ApplyInventoryData(lastDistrictID);
        }
    }

    /// <summary>
    /// Calculates a production multiplier based on god favors.
    /// Over 7 favor: positive effect.
    /// Under 3 favor: negative effect.
    /// </summary>
    public float GetProductionMultiplier(ResourceType type)
    {
        float multiplier = 1.0f;
        float baseEffect = 0.2f; // Base 20% change per god

        // Zeus affects everything (20% rate)
        multiplier += CalculateEffect(favorZeus, baseEffect * 0.2f);

        switch (type)
        {
            case ResourceType.Food:
                // Demeter & Poseidon affect food (100% rate)
                multiplier += CalculateEffect(favorDemeter, baseEffect);
                multiplier += CalculateEffect(favorPoseidon, baseEffect);
                // Artemis affects food (50% rate)
                multiplier += CalculateEffect(favorArtemis, baseEffect * 0.5f);
                break;

            case ResourceType.Stone:
            case ResourceType.Gold:
                // Hephaestus affects gold and stone (100% rate)
                multiplier += CalculateEffect(favorHephaestus, baseEffect);
                break;

            case ResourceType.Ichor:
                // Currently only Zeus affects Ichor (already handled above)
                break;
        }

        // Clamp multiplier so it doesn't go below 0 (negative production)
        return Mathf.Max(0.1f, multiplier);
    }

    private float CalculateEffect(int favor, float effectScale)
    {
        if (favor > 7) return effectScale;
        if (favor < 3) return -effectScale;
        return 0f;
    }
}
