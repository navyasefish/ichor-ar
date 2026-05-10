using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class FarmingManager : MonoBehaviour
{
  // ---------------------------------------------------------------
  // Inspector — assign the same ARRaycastManager as BuildingPlacementManager
  // ---------------------------------------------------------------
  [Header("AR")]
  [SerializeField] private ARRaycastManager arRaycastManager;

  [Header("Soil")]
  [SerializeField] private GameObject soilTilePrefab;

  [Header("Crops")]
  [SerializeField] private List<CropDefinition> availableCrops;

  // ---------------------------------------------------------------
  // Mode
  // ---------------------------------------------------------------
  public enum FarmMode { None, PlacingSoil, PlantingCrop, Harvesting }
  public FarmMode CurrentMode { get; private set; } = FarmMode.None;

  // ---------------------------------------------------------------
  // State
  // ---------------------------------------------------------------
  private GridManager gridManager;
  private CropDefinition selectedCrop = null;
  private Dictionary<Vector2Int, SoilTile> soilTileMap = new Dictionary<Vector2Int, SoilTile>();
  private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

  // ---------------------------------------------------------------
  // Called by SurfaceSelectionManager and UIManager
  // ---------------------------------------------------------------
  public void SetGridManager(GridManager grid)
  {
    gridManager = grid;
    Debug.Log("[FarmingManager] GridManager assigned.");
  }

  // ---------------------------------------------------------------
  // Mode entry points
  // ---------------------------------------------------------------
  public void StartSoilPlacement()
  {
    CurrentMode = FarmMode.PlacingSoil;
    selectedCrop = null;
    // Re-enable raycast manager exactly like scanning does
    if (arRaycastManager != null) arRaycastManager.enabled = true;
    Debug.Log("[FarmingManager] Mode: PlacingSoil");
  }

  public void StartCropPlanting(CropDefinition crop)
  {
    if (crop == null) { Debug.LogError("[FarmingManager] Null crop."); return; }
    selectedCrop = crop;
    CurrentMode = FarmMode.PlantingCrop;
    if (arRaycastManager != null) arRaycastManager.enabled = true;
    Debug.Log($"[FarmingManager] Mode: PlantingCrop ({crop.cropName})");
  }

  public void StartCropPlantingByName(string cropName)
  {
    CropDefinition found = availableCrops.Find(c => c.cropName == cropName);
    if (found == null) { Debug.LogError($"[FarmingManager] Crop '{cropName}' not found."); return; }
    StartCropPlanting(found);
  }

  public void StartHarvesting()
  {
    CurrentMode = FarmMode.Harvesting;
    selectedCrop = null;
    if (arRaycastManager != null) arRaycastManager.enabled = true;
    Debug.Log("[FarmingManager] Mode: Harvesting");
  }

  public void CancelMode()
  {
    CurrentMode = FarmMode.None;
    selectedCrop = null;
    Debug.Log("[FarmingManager] Mode cancelled.");

    UIManager ui = FindObjectOfType<UIManager>();
    if (ui != null) ui.OnPlacementFinished();
  }

  // ---------------------------------------------------------------
  // Update — exact same pattern as BuildingPlacementManager
  // ---------------------------------------------------------------
  private void Update()
  {
    if (CurrentMode == FarmMode.None) return;
    if (gridManager == null) return;

    // Get screen position — same as BPM
    Vector2 screenPos = Input.touchCount > 0
        ? Input.GetTouch(0).position
        : (Vector2)Input.mousePosition;

    // AR raycast — same call as BPM
    if (!arRaycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
      return;

    Vector3 worldPos = hits[0].pose.position;

    // Block UI taps — same as BPM terrain handling
    bool overUI = Input.touchCount > 0
        ? EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
        : EventSystem.current.IsPointerOverGameObject();
    if (overUI) return;

    // Only act on tap — same as BPM HandleTerrainPlacement
    bool tapped = (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
               || Input.GetMouseButtonDown(0);
    if (!tapped) return;

    // Find nearest tile — identical to BPM.FindNearestTile
    GridTile tile = FindNearestTile(worldPos);
    if (tile == null) return;

    Debug.Log($"[FarmingManager] Tapped tile {tile.coordinate} | mode={CurrentMode} | occupied={tile.isOccupied} | occupant={tile.Occupant}");

    switch (CurrentMode)
    {
      case FarmMode.PlacingSoil: HandleSoilPlacement(tile); break;
      case FarmMode.PlantingCrop: HandleCropPlanting(tile); break;
      case FarmMode.Harvesting: HandleHarvest(tile); break;
    }
  }

  // ---------------------------------------------------------------
  // Nearest tile — identical to BPM.FindNearestTile
  // ---------------------------------------------------------------
  private GridTile FindNearestTile(Vector3 worldPos)
  {
    float closestDist = Mathf.Infinity;
    GridTile closestTile = null;

    foreach (GridTile tile in gridManager.GetComponentsInChildren<GridTile>())
    {
      float dist = Vector3.Distance(worldPos, tile.transform.position);
      if (dist < closestDist)
      {
        closestDist = dist;
        closestTile = tile;
      }
    }
    return closestTile;
  }

  // ---------------------------------------------------------------
  // Soil placement
  // ---------------------------------------------------------------
  private void HandleSoilPlacement(GridTile tile)
  {
    Vector2Int coord = tile.coordinate;

    if (tile.isOccupied)
    {
      Debug.Log($"[FarmingManager] Tile {coord} is occupied — cannot place soil.");
      return;
    }

    if (soilTileMap.ContainsKey(coord))
    {
      Debug.Log($"[FarmingManager] Soil already at {coord}.");
      return;
    }

    // Spawn at tile position, matching tile rotation — same offset as terrain (+Y 0.01)
    GameObject soilGO = Instantiate(
        soilTilePrefab,
        tile.transform.position + new Vector3(0, 0.01f, 0),
        Quaternion.identity
    );

    SoilTile soilTile = soilGO.GetComponent<SoilTile>();
    if (soilTile == null)
    {
      Debug.LogError("[FarmingManager] soilTilePrefab missing SoilTile component!");
      Destroy(soilGO);
      return;
    }

    soilTile.GridCoordinate = coord;
    soilTileMap[coord] = soilTile;
    tile.SetOccupied(true, GridTile.OccupantType.Soil);

    Debug.Log($"[FarmingManager] Soil placed at {coord}.");
  }

  // ---------------------------------------------------------------
  // Crop planting
  // ---------------------------------------------------------------
  private void HandleCropPlanting(GridTile tile)
  {
    Vector2Int coord = tile.coordinate;

    if (!soilTileMap.TryGetValue(coord, out SoilTile soilTile))
    {
      Debug.Log($"[FarmingManager] No soil at {coord} — place soil first.");
      return;
    }

    if (soilTile.State != SoilTile.SoilState.Empty)
    {
      Debug.Log($"[FarmingManager] Soil at {coord} already has a crop.");
      return;
    }

    if (selectedCrop == null)
    {
      Debug.LogError("[FarmingManager] No crop selected.");
      return;
    }

    if (!BarnInventory.Instance.TryConsume(selectedCrop.cropName, selectedCrop.seedCost))
    {
      Debug.LogWarning($"[FarmingManager] Not enough {selectedCrop.cropName} seeds.");
      return;
    }

    soilTile.TryPlant(selectedCrop);
    Debug.Log($"[FarmingManager] Planted {selectedCrop.cropName} at {coord}.");
  }

  // ---------------------------------------------------------------
  // Harvest
  // ---------------------------------------------------------------
  private void HandleHarvest(GridTile tile)
  {
    Vector2Int coord = tile.coordinate;

    if (!soilTileMap.TryGetValue(coord, out SoilTile soilTile))
    {
      Debug.Log($"[FarmingManager] No soil at {coord}.");
      return;
    }

    if (soilTile.State != SoilTile.SoilState.ReadyToHarvest)
    {
      Debug.Log($"[FarmingManager] Crop at {coord} not ready yet. State={soilTile.State}");
      return;
    }

    CropDefinition harvested = soilTile.Harvest();
    if (harvested == null) return;

    BarnInventory.Instance.Add(harvested.cropName, harvested.harvestYield);
    Debug.Log($"[FarmingManager] +{harvested.harvestYield} {harvested.cropName} added to barn.");
  }
}