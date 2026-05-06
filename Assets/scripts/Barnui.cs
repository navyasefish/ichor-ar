using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Populates the barn panel with crop/item counts from BarnInventory.
/// Attach this to the Barn Panel GameObject.
///
/// Setup:
///   1. Assign barnPanel — the root panel shown when the barn is tapped.
///   2. Assign itemRowPrefab — a prefab with an Image (icon) + two TMP texts (name, count).
///   3. Assign contentParent — the Vertical Layout Group container inside a ScrollView.
/// </summary>
public class BarnUI : MonoBehaviour
{
  [Header("Panel")]
  [SerializeField] private GameObject barnPanel;

  [Header("Row Prefab")]
  [Tooltip("Prefab with: Image (cropIcon), TMP (cropName), TMP (cropCount)")]
  [SerializeField] private GameObject itemRowPrefab;

  [Tooltip("Content container (VerticalLayoutGroup inside ScrollView)")]
  [SerializeField] private Transform contentParent;

  [Header("Optional crop icon map")]
  [Tooltip("Used to show icons in the barn. Drag your CropDefinitions here so BarnUI can match names to icons.")]
  [SerializeField] private List<CropDefinition> cropDefinitions;

  // ---------------------------------------------------------------
  // Lifecycle
  // ---------------------------------------------------------------
  private void OnEnable()
  {
    // Subscribe to inventory changes so UI stays live while panel is open
    if (BarnInventory.Instance != null)
      BarnInventory.Instance.OnInventoryChanged += RefreshUI;

    RefreshUI();
  }

  private void OnDisable()
  {
    if (BarnInventory.Instance != null)
      BarnInventory.Instance.OnInventoryChanged -= RefreshUI;
  }

  // ---------------------------------------------------------------
  // Public — call this from a BuildingPlacementManager / barn building tap
  // ---------------------------------------------------------------
  public void OpenBarnPanel()
  {
    if (barnPanel != null)
      barnPanel.SetActive(true);

    RefreshUI();
  }

  public void CloseBarnPanel()
  {
    if (barnPanel != null)
      barnPanel.SetActive(false);
  }

  // ---------------------------------------------------------------
  // Refresh
  // ---------------------------------------------------------------
  private void RefreshUI()
  {
    if (contentParent == null) return;

    // Clear existing rows
    foreach (Transform child in contentParent)
      Destroy(child.gameObject);

    if (BarnInventory.Instance == null) return;

    Dictionary<string, int> inventory = BarnInventory.Instance.GetAll();

    foreach (KeyValuePair<string, int> entry in inventory)
    {
      if (itemRowPrefab == null) break;

      GameObject row = Instantiate(itemRowPrefab, contentParent);

      // --- Set crop name ---
      TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
      if (texts.Length >= 1) texts[0].text = entry.Key;         // crop name
      if (texts.Length >= 2) texts[1].text = entry.Value.ToString(); // count

      // --- Set icon if available ---
      Image icon = row.GetComponentInChildren<Image>();
      if (icon != null)
      {
        CropDefinition def = cropDefinitions?.Find(c => c.cropName == entry.Key);
        if (def != null && def.cropIcon != null)
          icon.sprite = def.cropIcon;
      }
    }
  }
}