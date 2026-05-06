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

  private bool scanMode = false;
  private bool surfaceSelected = false;

  private GridManager activeGrid = null;
  public bool IsGridReady => activeGrid != null;
  public GridManager GetActiveGrid() => activeGrid;

  public void StartScanning()
  {
    if (IsGridReady)
    {
      Debug.Log("[SurfaceSelectionManager] Grid already exists, skipping scan.");
      return;
    }

    scanMode = true;
    surfaceSelected = false;

    planeManager.enabled = true;
    raycastManager.enabled = true;

    foreach (var plane in planeManager.trackables)
      plane.gameObject.SetActive(true);

    Debug.Log("[SurfaceSelectionManager] Scanning started.");
  }

  public void ToggleGrid()
  {
    if (activeGrid == null) return;

    bool currentlyActive = activeGrid.gameObject.activeSelf;
    activeGrid.gameObject.SetActive(!currentlyActive);

    Debug.Log($"[SurfaceSelectionManager] Grid toggled → {(!currentlyActive ? "visible" : "hidden")}");
  }

  public void StopScanning()
  {
    scanMode = false;
    Debug.Log("[SurfaceSelectionManager] Scanning stopped.");
  }

  public void Rescan()
  {
    surfaceSelected = false;
    scanMode = true;

    planeManager.enabled = true;
    raycastManager.enabled = true;

    foreach (var plane in planeManager.trackables)
      plane.gameObject.SetActive(true);

    Debug.Log("[SurfaceSelectionManager] Rescanning started.");
  }

  private void Update()
  {
    if (!scanMode || surfaceSelected)
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
        SelectSurface(plane);
    }
  }

  private void SelectSurface(ARPlane selectedPlane)
  {
    surfaceSelected = true;
    scanMode = false;

    Debug.Log("[SurfaceSelectionManager] Plane selected.");

    MeshRenderer r = selectedPlane.GetComponent<MeshRenderer>();
    if (r != null) r.material = selectedPlaneMaterial;

    Vector3 spawnPos = selectedPlane.transform.position;
    GameObject board = Instantiate(boardPrefab, spawnPos, hits[0].pose.rotation);

    Debug.Log($"[SurfaceSelectionManager] Board spawned at: {board.transform.position}");

    GridManager grid = board.GetComponentInChildren<GridManager>();

    if (grid != null)
    {
      grid.GenerateGrid();
      grid.CullTilesOutsidePlane(selectedPlane);

      activeGrid = grid;

      // Hand grid to BuildingPlacementManager
      BuildingPlacementManager bpm = FindObjectOfType<BuildingPlacementManager>();
      if (bpm != null)
        bpm.SetGridManager(grid);
      else
        Debug.LogError("[SurfaceSelectionManager] BuildingPlacementManager not found!");

      // Hand grid to FarmingManager
      FarmingManager fm = FindObjectOfType<FarmingManager>();
      if (fm != null)
        fm.SetGridManager(grid);
      else
        Debug.LogWarning("[SurfaceSelectionManager] FarmingManager not found — farming won't work.");

      UIManager ui = FindObjectOfType<UIManager>();
      if (ui != null)
        ui.OnScanComplete();
      else
        Debug.LogError("[SurfaceSelectionManager] UIManager not found!");
    }
    else
    {
      Debug.LogError("[SurfaceSelectionManager] GridManager not found on board prefab!");
    }

    // Disable plane detection — raycast stays usable for placement
    planeManager.enabled = false;
    raycastManager.enabled = false;

    foreach (var plane in planeManager.trackables)
    {
      if (plane != selectedPlane)
        plane.gameObject.SetActive(false);
    }
  }
}