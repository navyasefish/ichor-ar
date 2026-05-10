using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWalk : MonoBehaviour
{
    [Header("Settings")]
    public float spawnInterval = 15f;
    public bool spawnOnStart = true;

    private void Start()
    {
        if (spawnOnStart)
        {
            InvokeRepeating(nameof(TrySpawn), 2f, spawnInterval);
        }
    }

    private void TrySpawn()
    {
        if (WalkingManager.Instance == null)
        {
            DevTools.LogWarning("[RandomWalk] WalkingManager instance not found!");
            return;
        }

        if (!WalkingManager.Instance.CanSpawn())
        {
            // Don't spam log if limit reached, but good to know
            return;
        }

        RoadConnector startConnector = GetComponent<RoadConnector>();
        if (startConnector == null)
        {
            DevTools.LogWarning($"[RandomWalk] {gameObject.name} missing RoadConnector!");
            return;
        }

        if (!startConnector.isConnected)
        {
            return;
        }

        // Find a random destination building with a road connector
        RoadConnector[] allConnectors = FindObjectsOfType<RoadConnector>();
        List<RoadConnector> validDestinations = new List<RoadConnector>();

        foreach (var conn in allConnectors)
        {
            if (conn != startConnector && conn.isConnected)
            {
                validDestinations.Add(conn);
            }
        }

        if (validDestinations.Count == 0)
        {
            DevTools.LogWarning($"[RandomWalk] {gameObject.name} found no connected destinations.");
            return;
        }

        RoadConnector endConnector = validDestinations[Random.Range(0, validDestinations.Count)];

        DevTools.Log($"[RandomWalk] {gameObject.name} starting walk to {endConnector.gameObject.name}");
        StartCoroutine(SpawnAndWalk(startConnector, endConnector));
    }

    private IEnumerator SpawnAndWalk(RoadConnector start, RoadConnector end)
    {
        GridManager grid = FindObjectOfType<GridManager>();
        if (grid == null)
        {
            DevTools.LogError("[RandomWalk] GridManager not found in scene!");
            yield break;
        }

        // Find path
        List<Vector2Int> gridPath = FindRoadPath(grid, start.gridCoordinate, end.gridCoordinate);
        if (gridPath == null || gridPath.Count == 0)
        {
            DevTools.LogWarning($"[RandomWalk] No road path found from {start.gridCoordinate} to {end.gridCoordinate}");
            yield break;
        }

        if (WalkingManager.Instance.walkerPrefabs == null || WalkingManager.Instance.walkerPrefabs.Length == 0)
        {
            DevTools.LogError("[RandomWalk] No walker prefabs assigned in WalkingManager!");
            yield break;
        }

        // Select random prefab
        GameObject prefab = WalkingManager.Instance.walkerPrefabs[Random.Range(0, WalkingManager.Instance.walkerPrefabs.Length)];
        
        // Spawn
        GameObject walker = Instantiate(prefab, transform.position, Quaternion.identity);
        WalkingManager.Instance.RegisterWalker();

        // Apply scale
        float scale = Random.Range(WalkingManager.Instance.minScale, WalkingManager.Instance.maxScale);
        walker.transform.localScale = Vector3.one * scale;

        // Set speed
        float speed = Random.Range(WalkingManager.Instance.minSpeed, WalkingManager.Instance.maxSpeed);

        // Convert grid path to world points
        List<Vector3> worldPath = new List<Vector3>();
        foreach (var coord in gridPath)
        {
            GridTile tile = grid.GetTile(coord);
            if (tile != null) worldPath.Add(tile.transform.position);
        }

        // Move along path
        foreach (Vector3 target in worldPath)
        {
            Vector3 posStart = walker.transform.position;
            float dist = Vector3.Distance(posStart, target);
            float duration = dist / speed;
            float elapsed = 0;

            while (elapsed < duration)
            {
                if (walker == null) yield break; // Safety if destroyed elsewhere
                walker.transform.position = Vector3.Lerp(posStart, target, elapsed / duration);

                Vector3 dir = target - posStart;
                if (dir != Vector3.zero)
                {
                    walker.transform.rotation = Quaternion.Slerp(walker.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
            if (walker != null) walker.transform.position = target;
        }

        // Cleanup
        if (walker != null)
        {
            Destroy(walker);
            WalkingManager.Instance.UnregisterWalker();
        }
    }

    // Pathfinding logic (BFS) - Reused from DeliverAnim
    private List<Vector2Int> FindRoadPath(GridManager grid, Vector2Int start, Vector2Int end)
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
                if (tile != null)
                {
                    bool isRoad = tile.placedObject != null && tile.placedObject.GetComponent<Road>() != null;
                    if (next == end || isRoad)
                    {
                        queue.Enqueue(next);
                        cameFrom[next] = current;
                    }
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
