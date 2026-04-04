using System.Collections.Generic;
using UnityEngine;

public class FlagLoader : MonoBehaviour
{
  [SerializeField] private GameObject flagPrefab;

  private void Start()
  {
    LoadFlags();
  }

  private void LoadFlags()
  {
    List<FlagSaveData> flags =
        SaveSystem.Instance.GetSavedFlags();

    foreach (FlagSaveData flag in flags)
    {
      Vector3 pos = flag.position;
      Quaternion rot =
          Quaternion.Euler(flag.rotation);

      Instantiate(flagPrefab, pos, rot);

      Debug.Log("Loaded Flag: " + flag.flagId);
    }
  }
}