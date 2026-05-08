using System.Collections.Generic;
using UnityEngine;

public class FlagLoader : MonoBehaviour
{
  public static FlagLoader Instance;
  [SerializeField] private GameObject flagPrefab;

  private void Awake()
  {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
  }

  public void LoadFlags(GridManager grid)
  {
    List<FlagSaveData> flags =
        SaveSystem.Instance.GetSavedFlags();

    if (flags == null || flags.Count == 0)
    {
        DevTools.LogWarning("[LOAD] No flags found in save file to load.");
        return;
    }

    DevTools.Log($"[LOAD] Found {flags.Count} flags in save file. Starting instantiation...");
    int loadedCount = 0;

    foreach (FlagSaveData flag in flags)
    {
      GridTile tile = grid.GetTile(flag.gridCoord);
      if (tile == null)
      {
          DevTools.LogWarning($"[LOAD] Could not find tile at {flag.gridCoord} for flag {flag.flagId}");
          continue;
      }

      Vector3 localRot = flag.rotation;

      // Parent to grid.transform (the board) instead of the tile to avoid inheriting tile's 90-degree rotation
      GameObject flagObj = Instantiate(flagPrefab, grid.transform);
      flagObj.transform.localPosition = tile.transform.localPosition;
      flagObj.transform.localRotation = Quaternion.Euler(localRot);
      flagObj.transform.localScale = Vector3.one;
      
      tile.isOccupied = true;
      tile.placedObject = flagObj;
      DistrictFlag df = flagObj.GetComponent<DistrictFlag>();
      if (df != null)
      {
        df.districtID = flag.districtID;
      }

      DevTools.Log($"[LOAD] Successfully snapped flag: {flag.flagId} to tile {flag.gridCoord}");
      loadedCount++;
    }

    DevTools.Log($"[LOAD] Finished loading. Total flags spawned: {loadedCount}");
    DevTools.SetStatus($"Successfully loaded {loadedCount} flags.");
  }
}