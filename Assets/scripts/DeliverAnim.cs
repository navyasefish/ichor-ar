using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliverAnim : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 1.0f;
    public float waitTime = 2.0f;

    private List<Vector3> pathPoints = new List<Vector3>();
    private string districtID;
    private Vector3 originalPos;
    private BuildingResourceGenerator owner;
    private int deliveryAmount;

    /// <summary>
    /// Starts the delivery animation from a building to its district flag.
    /// </summary>
    public static void StartDelivery(GameObject modelPrefab, Vector3 startWorldPos, Vector2Int startGridCoord, string dID, float speed = 0.5f, BuildingResourceGenerator callbackOwner = null, int amount = 0)
    {
        DistrictFlag flag = DistrictManager.Instance.GetFlagByID(dID);
        if (flag == null)
        {
            DevTools.LogWarning($"[DeliverAnim] No flag found for district {dID}");
            callbackOwner?.OnDeliveryComplete(); // Ensure we unlock if we can't start
            return;
        }

        GridManager grid = FindObjectOfType<GridManager>();
        if (grid == null)
        {
            callbackOwner?.OnDeliveryComplete();
            return;
        }

        // Find the road tile adjacent to the flag
        Vector2Int endGridCoord = FindRoadNearFlag(grid, flag);
        
        // Find path along roads
        List<Vector2Int> gridPath = FindRoadPath(grid, startGridCoord, endGridCoord);
        if (gridPath == null || gridPath.Count == 0)
        {
            DevTools.LogWarning("[DeliverAnim] No road path found to flag!");
            callbackOwner?.OnDeliveryComplete();
            return;
        }

        // Instantiate the model
        GameObject deliveryObj = Instantiate(modelPrefab, startWorldPos, Quaternion.identity);
        
        // Use existing DeliverAnim if prefab has it, otherwise add it
        DeliverAnim anim = deliveryObj.GetComponent<DeliverAnim>();
        if (anim == null)
        {
            anim = deliveryObj.AddComponent<DeliverAnim>();
        }

        anim.originalPos = startWorldPos;
        anim.districtID = dID;
        anim.moveSpeed = speed; // Set the translation speed
        anim.owner = callbackOwner;
        anim.deliveryAmount = amount;
        
        DevTools.Log($"[DeliverAnim] Starting delivery for {dID} with amount {amount} and speed {speed}");

        // Convert grid path to world points (centers of tiles)
        anim.pathPoints = new List<Vector3>();
        foreach (Vector2Int coord in gridPath)
        {
            GridTile tile = grid.GetTile(coord);
            if (tile != null) anim.pathPoints.Add(tile.transform.position);
        }

        anim.StartCoroutine(anim.DeliveryRoutine());
    }

    private IEnumerator DeliveryRoutine()
    {
        // 1. Move to flag
        yield return StartCoroutine(MoveAlongPath(pathPoints));

        // 🔹 NEW: DELIVER RESOURCES UPON REACHING FLAG
        if (owner != null)
        {
            owner.DeliverResources(deliveryAmount);
        }

        // 2. Wait
        yield return new WaitForSeconds(waitTime);

        // 3. Move back (reverse path)
        List<Vector3> reversePath = new List<Vector3>(pathPoints);
        reversePath.Reverse();
        yield return StartCoroutine(MoveAlongPath(reversePath));

        // 4. Return to exact original pos if first road wasn't exactly at building
        float elapsed = 0;
        Vector3 lastRoadPos = transform.position;
        while (elapsed < 0.5f)
        {
            transform.position = Vector3.Lerp(lastRoadPos, originalPos, elapsed / 0.5f);
            elapsed += Time.deltaTime * moveSpeed;
            yield return null;
        }

        // 5. Notify owner and Delete
        if (owner != null) owner.OnDeliveryComplete();
        Destroy(gameObject);
    }

    private IEnumerator MoveAlongPath(List<Vector3> points)
    {
        foreach (Vector3 target in points)
        {
            Vector3 start = transform.position;
            float dist = Vector3.Distance(start, target);
            float duration = dist / moveSpeed;
            float elapsed = 0;

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(start, target, elapsed / duration);
                
                // Rotate to face direction
                Vector3 dir = target - start;
                if (dir != Vector3.zero)
                {
                    float rotSpeed = 10f * Mathf.Max(1f, moveSpeed);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = target;
        }
    }

    private static Vector2Int FindRoadNearFlag(GridManager grid, DistrictFlag flag)
    {
        // Find the tile the flag is on
        GridTile[] allTiles = grid.GetComponentsInChildren<GridTile>();
        GridTile flagTile = null;
        float minDist = float.MaxValue;
        
        foreach (var tile in allTiles)
        {
            float d = Vector3.Distance(tile.transform.position, flag.transform.position);
            if (d < minDist)
            {
                minDist = d;
                flagTile = tile;
            }
        }

        if (flagTile == null) return Vector2Int.zero;

        // Check neighbors for a road
        Vector2Int[] neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in neighbors)
        {
            GridTile neighbor = grid.GetTile(flagTile.coordinate + dir);
            if (neighbor != null && neighbor.placedObject != null && neighbor.placedObject.GetComponent<Road>() != null)
            {
                return neighbor.coordinate;
            }
        }

        return flagTile.coordinate; // Fallback
    }

    private static List<Vector2Int> FindRoadPath(GridManager grid, Vector2Int start, Vector2Int end)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        
        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector2Int[] neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == end) break;

            foreach (Vector2Int dir in neighbors)
            {
                Vector2Int next = current + dir;
                if (cameFrom.ContainsKey(next)) continue;

                GridTile tile = grid.GetTile(next);
                if (tile != null && tile.placedObject != null && tile.placedObject.GetComponent<Road>() != null)
                {
                    queue.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(end)) return null;

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int curr = end;
        while (curr != start)
        {
            path.Add(curr);
            curr = cameFrom[curr];
        }
        path.Add(start);
        path.Reverse();
        return path;
    }
}
