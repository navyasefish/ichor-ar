using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

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

    previewBuilding.transform.position = anchorTile.transform.position;

    UpdateTileHighlights(anchorTile);
  }
  void UpdateTileHighlights(GridTile anchorTile)
  {
    ResetHighlightedTiles();

    List<Vector2Int> footprint = currentBuilding.GetFootprint();

    bool validPlacement = true;

    foreach (Vector2Int offset in footprint)
    {
      Vector2Int targetCoord = anchorTile.coordinate + offset;

      GridTile tile = gridManager.GetTile(targetCoord);

      if (tile == null || tile.isOccupied)
      {
        validPlacement = false;
        continue;
      }

      highlightedTiles.Add(tile);
    }

    foreach (GridTile tile in highlightedTiles)
    {
      if (validPlacement)
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
    if (previewBuilding == null)
      return;

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