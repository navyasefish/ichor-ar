using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SurfaceSelectionManager : MonoBehaviour
{
  [SerializeField] private ARRaycastManager raycastManager;
  [SerializeField] private ARPlaneManager planeManager;
  [SerializeField] private Material selectedPlaneMaterial;

  private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

  private bool surfaceSelected = false;

  private void Update()
  {
    if (surfaceSelected)
      return;

    Vector2 inputPosition;

    // Touch input
    if (Input.touchCount > 0)
    {
      Touch touch = Input.GetTouch(0);

      if (touch.phase != TouchPhase.Began)
        return;

      inputPosition = touch.position;
    }
    // Mouse click (editor)
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

    Debug.Log("Plane Selected");

    MeshRenderer renderer = selectedPlane.GetComponent<MeshRenderer>();

    if (renderer != null)
    {
      renderer.material = selectedPlaneMaterial;
    }

    // comment this to stop detection on click 
    planeManager.enabled = false;

    foreach (var plane in planeManager.trackables)
    {
      if (plane != selectedPlane)
        plane.gameObject.SetActive(false);
    }
  }
}