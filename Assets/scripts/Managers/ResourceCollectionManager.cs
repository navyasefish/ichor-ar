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
            BuildingResourceGenerator generator = hit.collider.GetComponentInParent<BuildingResourceGenerator>();
            if (generator != null)
            {
                generator.Harvest();
            }
        }
    }
}
