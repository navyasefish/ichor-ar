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

  private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

  private GameObject previewBuilding;
  private GridManager gridManager;
  private BuildingDefinition currentBuilding;

  private bool placementMode = false;
  private List<GridTile> highlightedTiles = new List<GridTile>();
  private bool currentPlacementValid = false;


  public void StartTestPlacement()
  {
    if (testBuildingPrefab == null)
    {
      Debug.LogError("Test building prefab not assigned!");
      return;
    }

    StartPlacement(testBuildingPrefab);
  }
  public void StartPlacement(GameObject buildingPrefab)
  {
    if (previewBuilding != null)
      Destroy(previewBuilding);

    previewBuilding = Instantiate(buildingPrefab);
    currentBuilding = previewBuilding.GetComponent<BuildingDefinition>();

    previewBuilding.name = "PreviewBuilding";

    placementMode = true;
  }

  void Update()
  {
    if (!placementMode || previewBuilding == null)
      return;

    // Ignore touches on UI
    if (Input.touchCount > 0)
    {
      if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        return;
    }
    else
    {
      if (EventSystem.current.IsPointerOverGameObject())
        return;
    }
    Vector2 screenPos;

    if (Input.touchCount > 0)
    {
      screenPos = Input.GetTouch(0).position;
    }
    else
    {
      screenPos = Input.mousePosition;
    }

    if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
    {
      Pose hitPose = hits[0].pose;

      MovePreview(hitPose.position);
    }
  }

  void MovePreview(Vector3 worldPos)
  {
    if (gridManager == null || currentBuilding == null)
      return;

    GridTile anchorTile = FindNearestTile(worldPos);

    if (anchorTile == null)
      return;

    float tileSize = gridManager.TileSize;

    int width = currentBuilding.GetWidth();
    int height = currentBuilding.GetHeight();

    Vector3 offset = new Vector3(
        (width - 1) * tileSize * 0.5f,
        0,
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
      if (currentPlacementValid)
        tile.SetValid();
      else
        tile.SetInvalid();
    }
  }
  void ResetHighlightedTiles()
  {
    foreach (GridTile tile in highlightedTiles)
    {
      tile.SetDefault();
    }

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
  public void ConfirmPlacement()
{
    if (!currentPlacementValid)
    {
        Debug.Log("Invalid placement!");
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
}

  public void CancelPlacement()
  {
    if (previewBuilding != null)
      Destroy(previewBuilding);

    placementMode = false;
  }
  public void SetGridManager(GridManager grid)
  {
    gridManager = grid;
  }
}