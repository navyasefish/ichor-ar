using UnityEngine;

/// <summary>
/// Sits on every instantiated soil tile.
/// Tracks whether the tile is empty, has a seed, is growing, or is ready to harvest.
/// Manages the growth timer and swaps stage prefabs automatically.
/// </summary>
public class SoilTile : MonoBehaviour
{
  public enum SoilState { Empty, Growing, ReadyToHarvest }

  // ---------------------------------------------------------------
  // State
  // ---------------------------------------------------------------
  public SoilState State { get; private set; } = SoilState.Empty;
  public CropDefinition PlantedCrop { get; private set; }
  public int CurrentStage { get; private set; }

  // Grid coordinate this soil tile occupies (assigned by FarmingManager on spawn)
  public Vector2Int GridCoordinate { get; set; }

  // ---------------------------------------------------------------
  // Internals
  // ---------------------------------------------------------------
  private float stageTimer = 0f;
  private GameObject currentStageObject = null;

  // ---------------------------------------------------------------
  // Public API
  // ---------------------------------------------------------------

  /// <summary>Called by FarmingManager when a crop seed is planted here.</summary>
  public bool TryPlant(CropDefinition crop)
  {
    if (State != SoilState.Empty)
    {
      Debug.LogWarning("[SoilTile] Already has a crop planted.");
      return false;
    }

    if (crop == null || crop.StageCount == 0)
    {
      Debug.LogError("[SoilTile] CropDefinition is null or has no stage prefabs!");
      return false;
    }

    PlantedCrop = crop;
    CurrentStage = 0;
    State = SoilState.Growing;
    stageTimer = 0f;

    SpawnStage(CurrentStage);

    Debug.Log($"[SoilTile] Planted {crop.cropName} at stage 0.");
    return true;
  }

  /// <summary>Called by FarmingManager when the player harvests this tile.</summary>
  public CropDefinition Harvest()
  {
    if (State != SoilState.ReadyToHarvest)
    {
      Debug.LogWarning("[SoilTile] Crop not ready to harvest yet.");
      return null;
    }

    CropDefinition harvested = PlantedCrop;

    // Clean up stage object
    if (currentStageObject != null)
      Destroy(currentStageObject);

    PlantedCrop = null;
    CurrentStage = 0;
    State = SoilState.Empty;
    stageTimer = 0f;

    Debug.Log($"[SoilTile] Harvested {harvested.cropName}.");
    return harvested;
  }

  // ---------------------------------------------------------------
  // Growth loop
  // ---------------------------------------------------------------
  private void Update()
  {
    if (State != SoilState.Growing) return;
    if (PlantedCrop == null) return;

    stageTimer += Time.deltaTime;

    float timeForThisStage = PlantedCrop.secondsPerStage[CurrentStage];

    if (stageTimer >= timeForThisStage)
    {
      stageTimer = 0f;
      AdvanceStage();
    }
  }

  private void AdvanceStage()
  {
    int nextStage = CurrentStage + 1;

    if (nextStage >= PlantedCrop.StageCount)
    {
      // Already at final stage — mark ready
      State = SoilState.ReadyToHarvest;
      Debug.Log($"[SoilTile] {PlantedCrop.cropName} is ready to harvest!");
      return;
    }

    CurrentStage = nextStage;
    SpawnStage(CurrentStage);

    if (PlantedCrop.IsFullyGrown(CurrentStage))
    {
      State = SoilState.ReadyToHarvest;
      Debug.Log($"[SoilTile] {PlantedCrop.cropName} fully grown → ready to harvest.");
    }
    else
    {
      Debug.Log($"[SoilTile] {PlantedCrop.cropName} advanced to stage {CurrentStage}.");
    }
  }

  private void SpawnStage(int stage)
  {
    // Remove previous stage object
    if (currentStageObject != null)
      Destroy(currentStageObject);

    GameObject prefab = PlantedCrop.stagePrefabs[stage];
    if (prefab == null)
    {
      Debug.LogWarning($"[SoilTile] Stage prefab {stage} is null for {PlantedCrop.cropName}.");
      return;
    }

    // Spawn slightly above the soil tile so it sits on top
    currentStageObject = Instantiate(prefab, transform.position + Vector3.up * 0.01f, Quaternion.identity, transform);
  }
}