using UnityEngine;

public class GridTile : MonoBehaviour
{
  [SerializeField] private Renderer tileRenderer;

  [SerializeField] private Material defaultMaterial;
  [SerializeField] private Material validMaterial;
  [SerializeField] private Material invalidMaterial;

  public Vector2Int coordinate;

  public bool isOccupied = false;

  // NEW: store terrain placed on this tile
  public GameObject terrainObject;
  public void SetDefault()
  {
    tileRenderer.material = defaultMaterial;
  }

  public void SetValid()
  {
    tileRenderer.material = validMaterial;
  }

  public void SetInvalid()
  {
    tileRenderer.material = invalidMaterial;
  }
}