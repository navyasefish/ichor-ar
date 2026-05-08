using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildingPlacementManager : MonoBehaviour
{
  [SerializeField] private ARRaycastManager raycastManager;
  [SerializeField] private Camera arCamera;
  [SerializeField] private GameObject testBuildingPrefab;
  [SerializeField] private GameObject currentPlaceablePrefab;

  private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

  private GameObject previewBuilding;
  private GridManager gridManager;
  private BuildingDefinition currentBuilding;
  private int currentRotation = 0;

  private bool placementMode = false;
  private bool placementUIShown = false;
  private List<GridTile> highlightedTiles = new List<GridTile>();
  private bool currentPlacementValid = false;

  // ---------------------------------------------------------------
  // Entry points — called by item buttons in the item panels.
  // After calling one of these, UIManager.OnItemSelected() should
  // also be called from the same button to switch to the
  // Confirm / Cancel / Rotate panel.
  // ---------------------------------------------------------------
  // ---------------------------------------------------------------
  // USE THESE on item buttons — single OnClick, no ordering issues.
  // Starts placement AND tells UIManager to show the placement panel,
  // in guaranteed order.
  // ---------------------------------------------------------------
  public void SelectBuilding(GameObject buildingPrefab)
  {
    StartPlacement(buildingPrefab);
    // Placement panel shows automatically on first grid hit in Update
  }

  public void SelectTerrain(GameObject terrainPrefab)
  {
    StartPlacement(terrainPrefab);
    // Placement panel shows automatically on first grid hit in Update
  }

  // ---------------------------------------------------------------
  // kept for legacy / direct code calls
  // ---------------------------------------------------------------
  public void StartTerrainPlacement(GameObject terrainPrefab)
  {
    StartPlacement(terrainPrefab);
  }

  public void StartBuildingPlacement(GameObject buildingPrefab)
  {
    StartPlacement(buildingPrefab);
  }

  public void StartTestPlacement()
  {
    if (testBuildingPrefab == null)
    {
      DevTools.LogError("[BuildingPlacementManager] testBuildingPrefab not assigned!");
      return;
    }
    StartPlacement(testBuildingPrefab);
  }

  public void StartPlacement(GameObject prefab)
  {
    if (previewBuilding != null)
      Destroy(previewBuilding);

    currentPlaceablePrefab = prefab;

    BuildingDefinition def = prefab.GetComponent<BuildingDefinition>();
    currentBuilding = def;
    currentRotation = 0;
    placementMode = true;
    placementUIShown = false;

    if (def.placeableType == PlaceableType.Building || def.placeableType == PlaceableType.Flag || def.placeableType == PlaceableType.Road)
    {
      previewBuilding = Instantiate(prefab);
      previewBuilding.name = "PreviewBuilding";
    }
  }

  // ---------------------------------------------------------------
  // Update — preview follows finger/mouse while in placement mode
  // ---------------------------------------------------------------
  void Update()
  {
    if (!placementMode || currentBuilding == null)
      return;

    Vector2 screenPos = Input.touchCount > 0
        ? Input.GetTouch(0).position
        : (Vector2)Input.mousePosition;

    if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
    {
      Pose hitPose = hits[0].pose;

      // First time the preview lands on the grid — switch to correct placement panel
      if (!placementUIShown)
      {
        placementUIShown = true;
        
        // ONLY show placement UI (Confirm/Cancel) for Buildings and Flags.
        // Terrain and Roads are instant tap-to-place and stay in the item selection panel.
        if (currentBuilding.placeableType == PlaceableType.Building || currentBuilding.placeableType == PlaceableType.Flag)
        {
          UIManager ui = FindObjectOfType<UIManager>();
          if (ui != null) ui.OnItemSelected();
        }
      }

      if (currentBuilding.placeableType == PlaceableType.Terrain || currentBuilding.placeableType == PlaceableType.Road)
      {
        // Terrain/Road: tap freely to place/replace tiles — only block taps on UI buttons
        bool overUI = Input.touchCount > 0
            ? EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
            : EventSystem.current.IsPointerOverGameObject();
        if (!overUI)
        {
          if (currentBuilding.placeableType == PlaceableType.Terrain)
            HandleTerrainPlacement(hitPose.position);
          else
            HandleRoadPlacement(hitPose.position);
        }
        
        // Road still uses a preview to show where it will land
        if (currentBuilding.placeableType == PlaceableType.Road)
        {
            MovePreview(hitPose.position);
        }
      }
      else
      {
        // Building preview always tracks — never blocked by UI
        MovePreview(hitPose.position);
      }
    }
  }

  // ---------------------------------------------------------------
  // Terrain: place on tap
  // ---------------------------------------------------------------
  void HandleTerrainPlacement(Vector3 worldPos)
  {
    bool tapped = (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
               || Input.GetMouseButtonDown(0);

    if (!tapped) return;

    GridTile tile = FindNearestTile(worldPos);
    if (tile == null) return;

    if (tile.terrainObject != null)
      Destroy(tile.terrainObject);

    GameObject terrain = Instantiate(
        currentPlaceablePrefab,
        tile.transform.position + new Vector3(0, 0.01f, 0),
        Quaternion.identity
    );
    tile.terrainObject = terrain;
  }

  // ---------------------------------------------------------------
  // Road: place on tap, no overlap
  // ---------------------------------------------------------------
  void HandleRoadPlacement(Vector3 worldPos)
  {
    bool tapped = (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
               || Input.GetMouseButtonDown(0);

    if (!tapped) return;

    GridTile tile = FindNearestTile(worldPos);
    if (tile == null || tile.isOccupied) return;

    // 🔹 Trigger district logic BEFORE placing
    string inherited;
    if (!IsRoadPlacementValid(tile, out inherited))
    {
        DevTools.LogWarning($"[RoadPlacement] Blocked at {tile.coordinate}: Would connect multiple districts.");
        return; 
    }

    GameObject roadObj = Instantiate(
        currentPlaceablePrefab,
        tile.transform.position,
        Quaternion.identity
    );
    
    tile.isOccupied = true;
    tile.placedObject = roadObj;

    Road road = roadObj.GetComponent<Road>();
    if (road != null) road.districtID = inherited;
    
    if (inherited != "none")
    {
        PropagateDistrictID(tile.coordinate, inherited);
    }
    
    DevTools.Log($"[RoadPlacement] Road placed at {tile.coordinate}. District: {inherited}");
  }

  // ---------------------------------------------------------------
  // Building: move preview + highlight tiles
  // ---------------------------------------------------------------
  void MovePreview(Vector3 worldPos)
  {
    if (gridManager == null || currentBuilding == null) return;

    GridTile anchorTile = FindNearestTile(worldPos);
    if (anchorTile == null) return;

    float tileSize = gridManager.TileSize;
    int width = currentBuilding.GetWidth();
    int height = currentBuilding.GetHeight();

    Vector3 offset = new Vector3(
        (width - 1) * tileSize * 0.5f, 0,
        (height - 1) * tileSize * 0.5f
    );

    previewBuilding.transform.position = anchorTile.transform.position + offset;
    UpdateTileHighlights(anchorTile);
  }

  void UpdateTileHighlights(GridTile anchorTile)
  {
    ResetHighlightedTiles();

    List<Vector2Int> footprint = currentBuilding.GetFootprint();
    currentPlacementValid = true;

    foreach (Vector2Int offset in footprint)
    {
      Vector2Int coord = anchorTile.coordinate + offset;
      GridTile tile = gridManager.GetTile(coord);

      if (tile == null || !tile.gameObject.activeInHierarchy || tile.isOccupied)
      {
        currentPlacementValid = false;
        continue;
      }
      highlightedTiles.Add(tile);
    }

    // 🔹 NEW — Check for road/flag conflicts
    if (currentPlacementValid)
    {
        if (currentBuilding.placeableType == PlaceableType.Road)
        {
            string inherited;
            if (!IsRoadPlacementValid(anchorTile, out inherited))
                currentPlacementValid = false;
        }
        else if (currentBuilding.placeableType == PlaceableType.Flag)
        {
            DistrictFlag df = previewBuilding.GetComponent<DistrictFlag>();
            string dID = df != null ? df.districtID : "global";
            
            Vector2Int[] neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (Vector2Int dir in neighbors)
            {
                GridTile neighborTile = gridManager.GetTile(anchorTile.coordinate + dir);
                if (neighborTile != null && neighborTile.placedObject != null)
                {
                    Road road = neighborTile.placedObject.GetComponent<Road>();
                    if (road != null && road.districtID != "none" && road.districtID != dID)
                    {
                        currentPlacementValid = false;
                        break;
                    }
                }
            }
        }
    }

    foreach (GridTile tile in highlightedTiles)
    {
      if (currentPlacementValid) tile.SetValid();
      else tile.SetInvalid();
    }
  }

  void ResetHighlightedTiles()
  {
    foreach (GridTile tile in highlightedTiles)
      tile.SetDefault();
    highlightedTiles.Clear();
  }

  GridTile FindNearestTile(Vector3 position)
  {
    float closestDist = Mathf.Infinity;
    GridTile closestTile = null;

    foreach (var tileObj in gridManager.GetComponentsInChildren<GridTile>())
    {
      float dist = Vector3.Distance(position, tileObj.transform.position);
      if (dist < closestDist)
      {
        closestDist = dist;
        closestTile = tileObj;
      }
    }
    return closestTile;
  }

  // ---------------------------------------------------------------
  // Confirm — place the building, then return to item panel
  // ---------------------------------------------------------------
  public void ConfirmPlacement()
  {
    // Terrain confirm: keep all placed tiles, exit placement mode, return to item panel
    if (currentBuilding.placeableType == PlaceableType.Terrain)
    {
      placementMode = false;
      placementUIShown = false;
      NotifyPlacementFinished();
      return;
    }

    if (!currentPlacementValid)
    {
      DevTools.Log("[BuildingPlacementManager] Placement invalid, ignoring confirm.");
      return;
    }

    // Mark tiles occupied and store object reference
    foreach (GridTile tile in highlightedTiles)
    {
      tile.isOccupied = true;
      tile.placedObject = previewBuilding;
      tile.SetDefault();
    }

    // 🔹 NEW — detect flag placement BEFORE clearing preview
    if (currentBuilding.placeableType == PlaceableType.Flag)
    {
      // Convert to local coordinates relative to the grid board for persistence
      Vector3 localPos = gridManager.transform.InverseTransformPoint(previewBuilding.transform.position);
      Quaternion localRot = Quaternion.Inverse(gridManager.transform.rotation) * previewBuilding.transform.rotation;
      
      // Save grid coordinate for perfect alignment on reload
      Vector2Int gridCoord = highlightedTiles.Count > 0 ? highlightedTiles[0].coordinate : Vector2Int.zero;
      
      DistrictFlag df = previewBuilding.GetComponent<DistrictFlag>();
      string dID = df != null ? df.districtID : "global";

      // 🔹 NEW — Check if flag placement conflicts with existing road districts
      Vector2Int[] neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
      foreach (Vector2Int dir in neighbors)
      {
          GridTile neighborTile = gridManager.GetTile(highlightedTiles[0].coordinate + dir);
          if (neighborTile != null && neighborTile.placedObject != null)
          {
              Road road = neighborTile.placedObject.GetComponent<Road>();
              if (road != null && road.districtID != "none" && road.districtID != dID)
              {
                  DevTools.LogWarning($"[FlagPlacement] Blocked: Flag {dID} is adjacent to road in district {road.districtID}");
                  // Since we already instantiated and marked tiles, we might need to rollback?
                  // Actually, UpdateTileHighlights should have blocked this.
                  // Let's add the check there too.
                  Destroy(previewBuilding);
                  foreach (GridTile t in highlightedTiles) t.isOccupied = false;
                  highlightedTiles.Clear();
                  previewBuilding = null;
                  placementMode = false;
                  placementUIShown = false;
                  NotifyPlacementFinished();
                  return;
              }
          }
      }

      SaveSystem.Instance.SaveFlag(
          localPos,
          localRot,
          currentBuilding.name,
          dID,
          gridCoord
      );

      // 🔹 NEW — Backpropagate district info to adjacent roads
      foreach (Vector2Int dir in neighbors)
      {
          GridTile neighborTile = gridManager.GetTile(highlightedTiles[0].coordinate + dir);
          if (neighborTile != null && neighborTile.placedObject != null)
          {
              Road road = neighborTile.placedObject.GetComponent<Road>();
              if (road != null)
              {
                  PropagateDistrictID(neighborTile.coordinate, dID);
              }
          }
      }
    }

    // 🔹 NEW — Handle Building connection to road
    if (currentBuilding.placeableType == PlaceableType.Building)
    {
        RoadConnector connector = previewBuilding.GetComponent<RoadConnector>();
        if (connector != null)
        {
            connector.CheckConnection(gridManager, highlightedTiles[0], currentBuilding.GetFootprint());
        }
    }

    previewBuilding.name = "Placed " + currentBuilding.placeableType;
    highlightedTiles.Clear();
    previewBuilding = null;
    placementMode = false;
    placementUIShown = false;

    NotifyPlacementFinished();
  }

  // ---------------------------------------------------------------
  // Cancel — destroy preview, then return to item panel
  // ---------------------------------------------------------------
  public void CancelPlacement()
  {
    if (previewBuilding != null)
      Destroy(previewBuilding);

    ResetHighlightedTiles();
    placementMode = false;
    placementUIShown = false;

    NotifyPlacementFinished();
  }

  // ---------------------------------------------------------------
  // Rotate — cycles preview in 90° steps
  // ---------------------------------------------------------------
  public void RotateBuilding()
  {
    if (previewBuilding == null) return;

    currentRotation = (currentRotation + 90) % 360;
    previewBuilding.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
  }

  // ---------------------------------------------------------------
  // Misc
  // ---------------------------------------------------------------
  public void SetGridManager(GridManager grid)
  {
    gridManager = grid;
  }

  private bool IsRoadPlacementValid(GridTile anchorTile, out string inheritedDistrict)
  {
    inheritedDistrict = "none";
    if (gridManager == null || anchorTile == null) return false;

    HashSet<string> adjacentDistricts = new HashSet<string>();
    
    Vector2Int[] neighbors = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    foreach (Vector2Int dir in neighbors)
    {
        GridTile neighborTile = gridManager.GetTile(anchorTile.coordinate + dir);
        if (neighborTile != null && neighborTile.placedObject != null)
        {
            // Check for Flag
            DistrictFlag flag = neighborTile.placedObject.GetComponent<DistrictFlag>();
            if (flag != null)
            {
                adjacentDistricts.Add(flag.districtID);
            }
            else
            {
                // Check for Road
                Road road = neighborTile.placedObject.GetComponent<Road>();
                if (road != null && road.districtID != "none")
                {
                    adjacentDistricts.Add(road.districtID);
                }
            }
        }
    }

    if (adjacentDistricts.Count > 1)
    {
        DevTools.LogWarning("[RoadPlacement] Blocked: Adjacent to multiple districts.");
        return false;
    }

    if (adjacentDistricts.Count == 1)
    {
        foreach (string d in adjacentDistricts) inheritedDistrict = d;
    }

    return true; 
  }

  /// <summary>
  /// Backpropagates a district ID through a connected road network.
  /// Also updates any buildings adjacent to the network.
  /// </summary>
  public void PropagateDistrictID(Vector2Int startCoord, string dID)
  {
    if (gridManager == null || dID == "none") return;

    Queue<Vector2Int> queue = new Queue<Vector2Int>();
    HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
    queue.Enqueue(startCoord);
    visited.Add(startCoord);

    Vector2Int[] neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    while (queue.Count > 0)
    {
        Vector2Int current = queue.Dequeue();
        GridTile tile = gridManager.GetTile(current);
        if (tile == null || tile.placedObject == null) continue;

        Road road = tile.placedObject.GetComponent<Road>();
        if (road == null) continue;

        // Update the road itself
        road.districtID = dID;

        // Check all 4 neighbors for more roads or buildings
        foreach (Vector2Int dir in neighbors)
        {
            Vector2Int nextCoord = current + dir;
            if (visited.Contains(nextCoord)) continue;

            GridTile nextTile = gridManager.GetTile(nextCoord);
            if (nextTile == null || nextTile.placedObject == null) continue;

            // If it's a road, add to queue
            Road nextRoad = nextTile.placedObject.GetComponent<Road>();
            if (nextRoad != null)
            {
                // Safety check: don't overwrite if it's already a different district (existing rule)
                // However, if we are backpropagating from a flag, we want to unify the network.
                // The IsRoadPlacementValid already prevents connecting two different districts.
                queue.Enqueue(nextCoord);
                visited.Add(nextCoord);
            }
            
            // If it's a building, update its connector
            RoadConnector connector = nextTile.placedObject.GetComponent<RoadConnector>();
            if (connector != null)
            {
                connector.SetDistrict(dID);
            }
        }
    }
    
    DevTools.Log($"[DistrictPropagation] Completed propagation for district: {dID}");
  }

  private void NotifyPlacementFinished()
  {
    UIManager ui = FindObjectOfType<UIManager>();
    if (ui != null)
      ui.OnPlacementFinished();
    else
      DevTools.LogError("[BuildingPlacementManager] UIManager not found!");
  }
}