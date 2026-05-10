using UnityEngine;

[CreateAssetMenu(fileName = "NewCrop", menuName = "Farming/Crop Definition")]
public class CropDefinition : ScriptableObject
{
  [Header("Crop Info")]
  public string cropName = "Wheat";
  public Sprite cropIcon;

  [Header("Economy")]
  [Tooltip("How many seeds are consumed from barn when planting")]
  public int seedCost = 1;
  [Tooltip("How many of this crop are added to barn on harvest")]
  public int harvestYield = 3;

  [Header("Growth Stages")]
  [Tooltip("Prefabs in order: index 0 = just planted, last index = fully grown")]
  public GameObject[] stagePrefabs;

  [Tooltip("Real-time seconds each stage lasts before advancing to the next")]
  public float[] secondsPerStage;

  // Convenience: how many stages total (driven by stagePrefabs length)
  public int StageCount => stagePrefabs != null ? stagePrefabs.Length : 0;

  // The final stage index
  public int FinalStage => StageCount - 1;

  public bool IsFullyGrown(int stage) => stage >= FinalStage;

  private void OnValidate()
  {
    // Keep secondsPerStage array length in sync with stagePrefabs
    if (stagePrefabs == null) return;

    if (secondsPerStage == null || secondsPerStage.Length != stagePrefabs.Length)
    {
      float[] newTimes = new float[stagePrefabs.Length];
      for (int i = 0; i < newTimes.Length; i++)
      {
        newTimes[i] = (secondsPerStage != null && i < secondsPerStage.Length)
            ? secondsPerStage[i]
            : 30f; // default 30 seconds per stage
      }
      secondsPerStage = newTimes;
    }
  }
}