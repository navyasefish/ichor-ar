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

  private void Update()
  {
    if (surfaceSelected)
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
    Debug.Log($"[SurfaceSelectionManager] Plane center: {selectedPlane.transform.position}");
    Debug.Log($"[SurfaceSelectionManager] Hit pose position: {hits[0].pose.position}");

    MeshRenderer renderer = selectedPlane.GetComponent<MeshRenderer>();
    if (renderer != null)
      renderer.material = selectedPlaneMaterial;

    // Spawn board snapped to plane center, not just hit point
    // This ensures grid tiles align with the plane's coordinate space
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
    }
    else
    {
      Debug.LogError("[SurfaceSelectionManager] GridManager not found on board!");
    }

    planeManager.enabled = false;

    foreach (var plane in planeManager.trackables)
    {
      if (plane != selectedPlane)
        plane.gameObject.SetActive(false);
    }
  }
}