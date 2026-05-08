using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ResourceCollectionManager : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask buildingLayer;

    private void Update()
    {
        // Don't harvest if clicking on UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleInput(Input.mousePosition);
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleInput(Input.GetTouch(0).position);
        }
    }

    private void HandleInput(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, buildingLayer))
        {
            DevTools.Log($"[Harvest] Raycast hit: {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            BuildingResourceGenerator generator = hit.collider.GetComponentInParent<BuildingResourceGenerator>();
            if (generator != null)
            {
                DevTools.Log($"[Harvest] Triggering harvest on {generator.gameObject.name}");
                generator.Harvest();
            }
            else
            {
                DevTools.LogWarning($"[Harvest] Hit object {hit.collider.gameObject.name} but found no BuildingResourceGenerator in parents.");
            }
        }
        else
        {
            // Optional: log if we hit nothing at all on that layer
            // DevTools.Log("[Harvest] Raycast missed all objects on buildingLayer.");
        }
    }
}
