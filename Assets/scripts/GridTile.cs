using UnityEngine;

public class GridTile : MonoBehaviour
{
  // Coordinate of this tile inside the grid
  public Vector2Int coordinate;

  // Whether a building is occupying this tile
  public bool isOccupied = false;

  // Optional reference to the building occupying it (useful later)
  public GameObject occupyingBuilding;
}