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
    if (gridManager == null)
      return;
    GridTile nearestTile = FindNearestTile(worldPos);

    if (nearestTile == null)
      return;

    previewBuilding.transform.position = nearestTile.transform.position;
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

    previewBuilding.name = "Placed Building";

    placementMode = false;
    previewBuilding = null;
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