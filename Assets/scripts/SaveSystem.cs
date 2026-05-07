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
  public void SaveFlag(Vector3 position, Quaternion rotation, string flagId, string districtID)
  {
    SaveData data = LoadData();
 
    FlagSaveData flag = new FlagSaveData
    {
      flagId = flagId,
      districtID = districtID,
      position = position,
      rotation = rotation.eulerAngles
    };
 
    data.flags.Add(flag);
 
    string json = JsonUtility.ToJson(data, true);
    File.WriteAllText(savePath, json);
 
    DevTools.Log($"Flag saved: {flagId} in district {districtID}");
  }

  public void SaveAllData(SaveData data)
  {
    string json = JsonUtility.ToJson(data, true);
    File.WriteAllText(savePath, json);
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
      DevTools.Log("Save data cleared.");
    }
    else
    {
      DevTools.Log("No save file to clear.");
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
  public PlayerSaveData player = new PlayerSaveData();
  public List<DistrictInventorySaveData> districtInventories = new List<DistrictInventorySaveData>();
}

[System.Serializable]
public class DistrictInventorySaveData
{
  public string districtID;
  public int food;
  public int stone;
  public int gold;
  public int population;
  public int ichor;
}

[System.Serializable]
public class PlayerSaveData
{
  public int food;
  public int stone;
  public int gold;
  public int population;
  public int ichor;

  [Header("God Favors")]
  public int favorZeus;
  public int favorAres;
  public int favorArtemis;
  public int favorDemeter;
  public int favorPoseidon;
  public int favorHephaestus;
}

[System.Serializable]
public class FlagSaveData
{
  public string flagId;
  public string districtID;
  public Vector3 position;
  public Vector3 rotation;
}