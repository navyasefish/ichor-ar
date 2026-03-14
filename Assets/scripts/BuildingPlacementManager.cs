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
      Debug.LogError("[BuildingPlacementManager] testBuildingPrefab not assigned!");
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

    if (def.placeableType == PlaceableType.Building)
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
        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
        {
          if (currentBuilding.placeableType == PlaceableType.Terrain)
            ui.OnTerrainSelected();   // confirm + cancel only
          else
            ui.OnItemSelected();      // confirm + cancel + rotate
        }
      }

      if (currentBuilding.placeableType == PlaceableType.Terrain)
      {
        // Terrain: tap freely to place/replace tiles — only block taps on UI buttons
        bool overUI = Input.touchCount > 0
            ? EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
            : EventSystem.current.IsPointerOverGameObject();
        if (!overUI)
          HandleTerrainPlacement(hitPose.position);
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
      Debug.Log("[BuildingPlacementManager] Placement invalid, ignoring confirm.");
      return;
    }

    foreach (GridTile tile in highlightedTiles)
    {
      tile.isOccupied = true;
      tile.SetDefault();
    }

    previewBuilding.name = "Placed Building";
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

  private void NotifyPlacementFinished()
  {
    UIManager ui = FindObjectOfType<UIManager>();
    if (ui != null)
      ui.OnPlacementFinished();
    else
      Debug.LogError("[BuildingPlacementManager] UIManager not found!");
  }
}