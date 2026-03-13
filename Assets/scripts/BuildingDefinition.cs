using UnityEngine;
using System.Collections.Generic;

public class BuildingDefinition : MonoBehaviour
{
  public BuildingShape shape;

  public List<Vector2Int> GetFootprint()
  {
    List<Vector2Int> footprint = new List<Vector2Int>();

    switch (shape)
    {
      case BuildingShape.OneByOne:
        footprint.Add(new Vector2Int(0, 0));
        break;

      case BuildingShape.TwoByTwo:
        footprint.Add(new Vector2Int(0, 0));
        footprint.Add(new Vector2Int(1, 0));
        footprint.Add(new Vector2Int(0, 1));
        footprint.Add(new Vector2Int(1, 1));
        break;

      case BuildingShape.ThreeByThree:
        for (int x = 0; x < 3; x++)
          for (int z = 0; z < 3; z++)
            footprint.Add(new Vector2Int(x, z));
        break;

      case BuildingShape.LShape:
        footprint.Add(new Vector2Int(0, 0));
        footprint.Add(new Vector2Int(1, 0));
        footprint.Add(new Vector2Int(0, 1));
        break;

      case BuildingShape.TShape:
        footprint.Add(new Vector2Int(0, 0));
        footprint.Add(new Vector2Int(1, 0));
        footprint.Add(new Vector2Int(2, 0));
        footprint.Add(new Vector2Int(1, 1));
        break;
    }

    return footprint;
  }
}