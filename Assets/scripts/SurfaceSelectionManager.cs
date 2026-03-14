using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SurfaceSelectionManager : MonoBehaviour
{
  [SerializeField] private ARRaycastManager raycastManager;
  [SerializeField] private ARPlaneManager planeManager;
  [SerializeField] private Material selectedPlaneMaterial;
  [SerializeField] private GameObject boardPrefab;

  private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
  private bool surfaceSelected = false;
  private bool scanMode = false;

  private void Update()
  {
    if (surfaceSelected || !scanMode)
      return;

    Vector2 inputPosition;

    if (Input.touchCount > 0)
    {
      Touch touch = Input.GetTouch(0);
      if (touch.phase != TouchPhase.Began)
        return;
      inputPosition = touch.position;
    }
    else if (Input.GetMouseButtonDown(0))
    {
      inputPosition = Input.mousePosition;
    }
    else
    {
      return;
    }

    if (raycastManager.Raycast(inputPosition, hits, TrackableType.PlaneWithinPolygon))
    {
      ARPlane plane = planeManager.GetPlane(hits[0].trackableId);

      if (plane != null)
      {
        SelectSurface(plane);
      }
    }
  }

  private void SelectSurface(ARPlane selectedPlane)
  {
    surfaceSelected = true;

    Debug.Log("[SurfaceSelectionManager] Plane selected.");

    MeshRenderer renderer = selectedPlane.GetComponent<MeshRenderer>();
    if (renderer != null)
      renderer.material = selectedPlaneMaterial;

    Vector3 spawnPos = new Vector3(
        selectedPlane.transform.position.x,
        selectedPlane.transform.position.y,
        selectedPlane.transform.position.z
    );

    GameObject board = Instantiate(boardPrefab, spawnPos, hits[0].pose.rotation);

    Debug.Log($"[SurfaceSelectionManager] Board spawned at: {board.transform.position}");

    GridManager grid = board.GetComponentInChildren<GridManager>();

    if (grid != null)
    {
      grid.GenerateGrid();
      grid.CullTilesOutsidePlane(selectedPlane);

      UIManager ui = FindObjectOfType<UIManager>();

      if (ui != null)
      {
        ui.ShowPanel(ui.placementPanel);
      }

      BuildingPlacementManager placementManager = FindObjectOfType<BuildingPlacementManager>();

      if (placementManager != null)
      {
        placementManager.SetGridManager(grid);
      }
    }
    else
    {
      Debug.LogError("[SurfaceSelectionManager] GridManager not found on board!");
    }

    planeManager.enabled = false;
    raycastManager.enabled = false;

    foreach (var plane in planeManager.trackables)
    {
      if (plane != selectedPlane)
        plane.gameObject.SetActive(false);
    }
    scanMode = false;
  }
  
  public void Rescan()
  {
    surfaceSelected = false;

    planeManager.enabled = true;
    raycastManager.enabled = true;

    foreach (var plane in planeManager.trackables)
    {
      plane.gameObject.SetActive(true);
    }

    Debug.Log("Rescanning started.");
  }
  public void StartScanning()
  {
    scanMode = true;
    surfaceSelected = false;

    planeManager.enabled = true;
    raycastManager.enabled = true;

    foreach (var plane in planeManager.trackables)
    {
      plane.gameObject.SetActive(true);
    }

    Debug.Log("Scanning started");
  }

  public void StopScanning()
  {
    scanMode = false;
  }
}