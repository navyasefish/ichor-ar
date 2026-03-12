using UnityEngine;

public class GridManager : MonoBehaviour
{
  [SerializeField] private GameObject tilePrefab;
  [SerializeField] private int gridWidth = 12;
  [SerializeField] private int gridHeight = 12;
  [SerializeField] private float tileSize = 0.25f;

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

        Instantiate(tilePrefab, position, Quaternion.identity, transform);
      }
    }
  }
}