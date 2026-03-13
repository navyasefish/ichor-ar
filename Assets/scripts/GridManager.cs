using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;

public class GridManager : MonoBehaviour
{
  [SerializeField] private GameObject tilePrefab;
  [SerializeField] private int gridWidth = 12;
  [SerializeField] private int gridHeight = 12;
  [SerializeField] private float tileSize = 0.25f;

  private List<GameObject> spawnedTiles = new List<GameObject>();
  private Dictionary<Vector2Int, GridTile> gridTiles = new Dictionary<Vector2Int, GridTile>();

  public void GenerateGrid()
  {
    float gridOffsetX = (gridWidth * tileSize) / 2f;
    float gridOffsetZ = (gridHeight * tileSize) / 2f;

    for (int x = 0; x < gridWidth; x++)
    {
      for (int z = 0; z < gridHeight; z++)
      {
        Vector3 position = new Vector3(
            x * tileSize - gridOffsetX,
            0,
            z * tileSize - gridOffsetZ
        );

        GameObject tile = Instantiate(tilePrefab, position, Quaternion.Euler(90f, 0f, 0f), transform);
        // Get the GridTile component
        GridTile gridTile = tile.GetComponent<GridTile>();

        // Assign its coordinate
        gridTile.coordinate = new Vector2Int(x, z);

        spawnedTiles.Add(tile);
        gridTiles.Add(gridTile.coordinate, gridTile);
      }
    }

    Debug.Log($"[GridManager] GenerateGrid complete. Total tiles: {spawnedTiles.Count}");
  }

  public void CullTilesOutsidePlane(ARPlane plane)
  {
    if (plane == null)
    {
      Debug.LogError("[GridManager] Plane is null!");
      return;
    }

    Vector2[] boundary = plane.boundary.ToArray();
    Debug.Log($"[GridManager] Boundary points: {boundary.Length}");

    if (boundary.Length < 3)
    {
      Debug.LogWarning("[GridManager] Boundary too small, showing all tiles.");
      foreach (var t in spawnedTiles) t.SetActive(true);
      return;
    }

    int kept = 0, culled = 0;

    foreach (GameObject tile in spawnedTiles)
    {
      bool fullyInside = AllCornersInsidePlane(tile, boundary, plane);
      tile.SetActive(fullyInside);

      if (fullyInside)
      {
        kept++;

        // Snap tile Y to sit exactly on the plane surface
        Vector3 pos = tile.transform.position;
        pos.y = plane.transform.position.y + 0.001f;
        tile.transform.position = pos;

        Debug.Log($"[GridManager] Visible tile at: {tile.transform.position}");
      }
      else
      {
        culled++;
      }
    }

    Debug.Log($"[GridManager] Kept: {kept} | Hidden: {culled} | Total: {spawnedTiles.Count}");

    if (kept == 0)
    {
      Debug.LogWarning("[GridManager] ALL tiles culled! Diagnosing...");

      if (spawnedTiles.Count > 0)
      {
        Vector3 wp = spawnedTiles[0].transform.position;
        Vector3 lp = plane.transform.InverseTransformPoint(wp);
        Debug.LogWarning($"[GridManager] First tile world pos: {wp}");
        Debug.LogWarning($"[GridManager] First tile in plane local space: {lp}");
        Debug.LogWarning($"[GridManager] 2D point tested: ({lp.x:F3}, {lp.z:F3})");
        Debug.LogWarning($"[GridManager] Plane center: {plane.transform.position}");
        Debug.LogWarning($"[GridManager] Board center: {transform.position}");

        float offsetX = Mathf.Abs(transform.position.x - plane.transform.position.x);
        float offsetZ = Mathf.Abs(transform.position.z - plane.transform.position.z);
        Debug.LogWarning($"[GridManager] Board-to-plane offset: X={offsetX:F3} Z={offsetZ:F3}");
      }
    }
  }

  private bool AllCornersInsidePlane(GameObject tile, Vector2[] boundary, ARPlane plane)
  {
    float half = (tileSize / 2f) * 0.95f;
    Vector3 wp = tile.transform.position;
    float planeY = plane.transform.position.y;

    Vector3[] corners = new Vector3[]
    {
            new Vector3(wp.x - half, planeY, wp.z - half),
            new Vector3(wp.x + half, planeY, wp.z - half),
            new Vector3(wp.x + half, planeY, wp.z + half),
            new Vector3(wp.x - half, planeY, wp.z + half),
    };

    foreach (Vector3 corner in corners)
    {
      Vector3 local = plane.transform.InverseTransformPoint(corner);
      Vector2 point2D = new Vector2(local.x, local.z);

      if (!IsPointInsidePolygon(point2D, boundary))
        return false;
    }

    return true;
  }

  private bool IsPointInsidePolygon(Vector2 point, Vector2[] polygon)
  {
    int count = polygon.Length;
    bool inside = false;

    for (int i = 0, j = count - 1; i < count; j = i++)
    {
      if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
          (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) /
          (polygon[j].y - polygon[i].y) + polygon[i].x))
      {
        inside = !inside;
      }
    }

    return inside;
  }
  public GridTile GetTile(Vector2Int coordinate)
  {
    if (gridTiles.TryGetValue(coordinate, out GridTile tile))
      return tile;

    return null;
  }
}