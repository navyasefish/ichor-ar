using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
  public static SaveSystem Instance;

  private string savePath;

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else
    {
      Destroy(gameObject);
    }

    savePath = Path.Combine(Application.persistentDataPath, "save.json");
  }


  // FLAG SAVE
  public void SaveFlag(Vector3 position, Quaternion rotation, string flagId)
  {
    SaveData data = LoadData();

    FlagSaveData flag = new FlagSaveData
    {
      flagId = flagId,
      position = position,
      rotation = rotation.eulerAngles
    };

    data.flags.Add(flag);

    string json = JsonUtility.ToJson(data, true);
    File.WriteAllText(savePath, json);

    Debug.Log("Flag saved: " + flagId);
  }
  public List<FlagSaveData> GetSavedFlags()
  {
    SaveData data = LoadData();
    return data.flags;
  }
  public void ClearSaveData()
  {
    if (File.Exists(savePath))
    {
      File.Delete(savePath);
      Debug.Log("Save data cleared.");
    }
    else
    {
      Debug.Log("No save file to clear.");
    }
  }

  // LOAD DATA
  public SaveData LoadData()
  {
    if (File.Exists(savePath))
    {
      string json = File.ReadAllText(savePath);
      return JsonUtility.FromJson<SaveData>(json);
    }

    return new SaveData();
  }
}

[System.Serializable]
public class SaveData
{
  public List<FlagSaveData> flags = new List<FlagSaveData>();
}

[System.Serializable]
public class FlagSaveData
{
  public string flagId;
  public Vector3 position;
  public Vector3 rotation;
}