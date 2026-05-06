using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that holds all crop/item counts for the barn.
/// Lives as a DontDestroyOnLoad object (attach to the same GameObject as SaveSystem,
/// or to its own persistent manager GameObject).
/// </summary>
public class BarnInventory : MonoBehaviour
{
  public static BarnInventory Instance { get; private set; }

  // Internal: cropName → count
  private Dictionary<string, int> inventory = new Dictionary<string, int>();

  // Event fired whenever any count changes — BarnUI listens to this
  public event System.Action OnInventoryChanged;

  // ---------------------------------------------------------------
  // Lifecycle
  // ---------------------------------------------------------------
  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
      SeedStartingInventory();
    }
    else
    {
      Destroy(gameObject);
    }
  }

  /// <summary>Populate starting amounts. Add more crops here as you expand.</summary>
  private void SeedStartingInventory()
  {
    // Wheat seeds to start with
    Set("Wheat", 10);

    // Add more starting items here later, e.g.:
    // Set("Carrot", 10);
  }

  // ---------------------------------------------------------------
  // Public API
  // ---------------------------------------------------------------

  /// <summary>Returns current count of an item (0 if not present).</summary>
  public int Get(string itemName)
  {
    return inventory.TryGetValue(itemName, out int count) ? count : 0;
  }

  /// <summary>Adds amount to item count. Amount can be negative to remove.</summary>
  public void Add(string itemName, int amount)
  {
    if (!inventory.ContainsKey(itemName))
      inventory[itemName] = 0;

    inventory[itemName] = Mathf.Max(0, inventory[itemName] + amount);
    OnInventoryChanged?.Invoke();

    Debug.Log($"[BarnInventory] {itemName}: {inventory[itemName]} (changed by {amount})");
  }

  /// <summary>
  /// Tries to consume 'amount' of item. Returns true and deducts if enough stock.
  /// Returns false and changes nothing if insufficient.
  /// </summary>
  public bool TryConsume(string itemName, int amount)
  {
    int current = Get(itemName);
    if (current < amount)
    {
      Debug.LogWarning($"[BarnInventory] Not enough {itemName}. Have {current}, need {amount}.");
      return false;
    }

    inventory[itemName] = current - amount;
    OnInventoryChanged?.Invoke();

    Debug.Log($"[BarnInventory] Consumed {amount}x {itemName}. Remaining: {inventory[itemName]}");
    return true;
  }

  /// <summary>Directly sets a count (useful for initialisation and save/load).</summary>
  public void Set(string itemName, int amount)
  {
    inventory[itemName] = Mathf.Max(0, amount);
    OnInventoryChanged?.Invoke();
  }

  /// <summary>Returns a copy of the full inventory for display purposes.</summary>
  public Dictionary<string, int> GetAll()
  {
    return new Dictionary<string, int>(inventory);
  }
}