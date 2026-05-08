using UnityEngine;
using System.Collections.Generic;

public class RoadConnector : MonoBehaviour
{
    [Header("Connection Status")]
    public bool isConnected = false;
    public string districtID = "none";
    public Vector2Int gridCoordinate;

    /// <summary>
    /// Checks adjacent tiles for roads to establish a district connection.
    /// Called after building placement or when a nearby road is placed.
    /// </summary>
    public void CheckConnection(GridManager grid, GridTile anchorTile, List<Vector2Int> footprint)
    {
        isConnected = false;
        districtID = "none";

        if (grid == null || anchorTile == null) return;
        gridCoordinate = anchorTile.coordinate;

        HashSet<Vector2Int> occupiedCoords = new HashSet<Vector2Int>();
        foreach (Vector2Int offset in footprint)
        {
            occupiedCoords.Add(anchorTile.coordinate + offset);
        }

        Vector2Int[] neighbors = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (Vector2Int coord in occupiedCoords)
        {
            foreach (Vector2Int dir in neighbors)
            {
                Vector2Int neighborCoord = coord + dir;
                if (occupiedCoords.Contains(neighborCoord)) continue;

                GridTile neighborTile = grid.GetTile(neighborCoord);
                if (neighborTile != null && neighborTile.placedObject != null)
                {
                    Road road = neighborTile.placedObject.GetComponent<Road>();
                    if (road != null && road.districtID != "none")
                    {
                        isConnected = true;
                        districtID = road.districtID;
                        DevTools.Log($"[RoadConnector] Connected to road at {neighborCoord}. District: {districtID}");
                        return; // Connect to the first valid road found
                    }
                }
            }
        }
        
        DevTools.LogWarning("[RoadConnector] No adjacent roads found. Building is disconnected.");
    }

    public void SetDistrict(string dID)
    {
        districtID = dID;
        isConnected = (dID != "none");
        
        // Also update ResourceGenerator if present
        var generator = GetComponent<BuildingResourceGenerator>();
        if (generator != null)
        {
            generator.assignedDistrictID = dID;
        }

        // Also update ResourceConsumer if present
        var consumer = GetComponent<BuildingResourceConsumer>();
        if (consumer != null)
        {
            consumer.assignedDistrictID = dID;
        }
    }
}
