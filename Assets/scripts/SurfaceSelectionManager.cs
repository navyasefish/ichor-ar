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

  // Strict guards — nothing happens unless both are true
  private bool scanMode = false;
  private bool surfaceSelected = false;

  // ---------------------------------------------------------------
  // Only called from UIManager.OnCategorySelected — never auto-starts
  // ---------------------------------------------------------------
  public void StartScanning()
  {
    scanMode = true;
    surfaceSelected = false;

    planeManager.enabled = true;
    raycastManager.enabled = true;

    foreach (var plane in planeManager.trackables)
      plane.gameObject.SetActive(true);

    Debug.Log("[SurfaceSelectionManager] Scanning started.");
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

  // ---------------------------------------------------------------
  // Update — only active when scanMode == true AND no surface yet
  // ---------------------------------------------------------------
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

  // ---------------------------------------------------------------
  // Internal — runs once when the player taps a valid AR plane
  // ---------------------------------------------------------------
  private void SelectSurface(ARPlane selectedPlane)
  {
    surfaceSelected = true;
    scanMode = false;

    Debug.Log("[SurfaceSelectionManager] Plane selected.");

    // Highlight the chosen plane
    MeshRenderer r = selectedPlane.GetComponent<MeshRenderer>();
    if (r != null) r.material = selectedPlaneMaterial;

    // Spawn the board at the plane's position
    Vector3 spawnPos = selectedPlane.transform.position;
    GameObject board = Instantiate(boardPrefab, spawnPos, hits[0].pose.rotation);

    Debug.Log($"[SurfaceSelectionManager] Board spawned at: {board.transform.position}");

    GridManager grid = board.GetComponentInChildren<GridManager>();

    if (grid != null)
    {
      grid.GenerateGrid();
      grid.CullTilesOutsidePlane(selectedPlane);

      // Hand grid reference to BuildingPlacementManager
      BuildingPlacementManager bpm = FindObjectOfType<BuildingPlacementManager>();
      if (bpm != null)
        bpm.SetGridManager(grid);
      else
        Debug.LogError("[SurfaceSelectionManager] BuildingPlacementManager not found!");

      // Tell UIManager grid is ready → it will show the correct item panel
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

    // Disable further plane detection
    planeManager.enabled = false;
    raycastManager.enabled = false;

    foreach (var plane in planeManager.trackables)
    {
      if (plane != selectedPlane)
        plane.gameObject.SetActive(false);
    }
  }
}