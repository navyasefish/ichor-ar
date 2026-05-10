using UnityEngine;

public class GridTile : MonoBehaviour
{
  // Set by GridManager during GenerateGrid()
  public Vector2Int coordinate;

  // Used by BuildingPlacementManager for terrain placement
  public GameObject terrainObject;

  // Used by BuildingPlacementManager to block building overlap
  public bool isOccupied = false;

  // Farming extension: what type of object is on this tile
  // BuildingPlacementManager ignores this � only FarmingManager uses it
  public enum OccupantType { None, Building, Terrain, Soil }
  public OccupantType Occupant { get; private set; } = OccupantType.None;

  /// <summary>Used by FarmingManager to place soil and record its type.</summary>
  public void SetOccupied(bool occupied, OccupantType type = OccupantType.None)
  {
    isOccupied = occupied;
    Occupant = occupied ? type : OccupantType.None;
  }

  /// <summary>True only if this tile has soil � crops can be planted here.</summary>
  public bool HasSoil => Occupant == OccupantType.Soil;

  // ---------------------------------------------------------------
  // Highlight states � called by BuildingPlacementManager
  // ---------------------------------------------------------------
  [Header("Highlight Materials")]
  [SerializeField] private Material defaultMaterial;
  [SerializeField] private Material validMaterial;
  [SerializeField] private Material invalidMaterial;

  private MeshRenderer meshRenderer;

  private void Awake()
  {
    meshRenderer = GetComponent<MeshRenderer>();
  }

  // NEW: store objects placed on this tile
  public GameObject terrainObject;
  public GameObject placedObject;
  public void SetDefault()
  {
    if (meshRenderer != null && defaultMaterial != null)
      meshRenderer.material = defaultMaterial;
  }

  public void SetValid()
  {
    if (meshRenderer != null && validMaterial != null)
      meshRenderer.material = validMaterial;
  }

  public void SetInvalid()
  {
    if (meshRenderer != null && invalidMaterial != null)
      meshRenderer.material = invalidMaterial;
  }
}