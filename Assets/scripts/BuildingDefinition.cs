using System.Collections.Generic;
using UnityEngine;

public enum PlaceableType
{
  Terrain,
  Building
}


public class BuildingDefinition : MonoBehaviour
{
  public PlaceableType placeableType = PlaceableType.Building;

  public enum BuildingShape
    {
        OneByOne,
        TwoByTwo,
        ThreeByThree,
        TShape,
        LShape
    }

    [Header("Building Settings")]
    public BuildingShape shape;

    [Tooltip("Size of one grid tile")]
    public float tileSize = 0.2f;

  public List<Vector2Int> GetFootprint()
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        switch (shape)
        {
            case BuildingShape.OneByOne:
                tiles.Add(new Vector2Int(0, 0));
                break;

            case BuildingShape.TwoByTwo:
                tiles.Add(new Vector2Int(0, 0));
                tiles.Add(new Vector2Int(1, 0));
                tiles.Add(new Vector2Int(0, 1));
                tiles.Add(new Vector2Int(1, 1));
                break;

            case BuildingShape.ThreeByThree:
                for (int x = 0; x < 3; x++)
                    for (int z = 0; z < 3; z++)
                        tiles.Add(new Vector2Int(x, z));
                break;

            case BuildingShape.TShape:
                tiles.Add(new Vector2Int(0, 1));
                tiles.Add(new Vector2Int(1, 1));
                tiles.Add(new Vector2Int(2, 1));
                tiles.Add(new Vector2Int(1, 0));
                break;

            case BuildingShape.LShape:
                tiles.Add(new Vector2Int(0, 0));
                tiles.Add(new Vector2Int(0, 1));
                tiles.Add(new Vector2Int(1, 0));
                break;
        }

        return tiles;
    }

    public int GetWidth()
    {
        int maxX = 0;

        foreach (var tile in GetFootprint())
        {
            if (tile.x > maxX)
                maxX = tile.x;
        }

        return maxX + 1;
    }

    public int GetHeight()
    {
        int maxZ = 0;

        foreach (var tile in GetFootprint())
        {
            if (tile.y > maxZ)
                maxZ = tile.y;
        }

        return maxZ + 1;
    }

    public Vector3 GetCenterOffset()
    {
        float offsetX = (GetWidth() - 1) * tileSize * 0.5f;
        float offsetZ = (GetHeight() - 1) * tileSize * 0.5f;

        return new Vector3(offsetX, 0, offsetZ);
    }
}