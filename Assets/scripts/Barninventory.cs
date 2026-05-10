using System.Collections.Generic;
using UnityEngine;

public class BarnInventory : MonoBehaviour
{
  public static BarnInventory Instance { get; private set; }

  private Dictionary<string, int> inventory = new Dictionary<string, int>();

  public event System.Action OnInventoryChanged;

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

  private void SeedStartingInventory()
  {
    Set("Wheat", 50);
    Set("Carrot", 60);
    Set("Turnip", 50);
    Set("Corn", 50);
  }

  public int Get(string itemName)
  {
    return inventory.TryGetValue(itemName, out int count) ? count : 0;
  }

  public void Add(string itemName, int amount)
  {
    if (!inventory.ContainsKey(itemName))
      inventory[itemName] = 0;

    inventory[itemName] = Mathf.Max(0, inventory[itemName] + amount);
    OnInventoryChanged?.Invoke();

    Debug.Log($"[BarnInventory] {itemName}: {inventory[itemName]} (changed by {amount})");
  }

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

  public void Set(string itemName, int amount)
  {
    inventory[itemName] = Mathf.Max(0, amount);
    OnInventoryChanged?.Invoke();
  }

  public Dictionary<string, int> GetAll()
  {
    return new Dictionary<string, int>(inventory);
  }
}