using UnityEngine;

public class BarnBuilding : MonoBehaviour
{
  private void OnMouseDown()
  {
    // true = include inactive GameObjects in search
    BarnUI barnUI = FindObjectOfType<BarnUI>(true);
    if (barnUI != null)
      barnUI.OpenBarnPanel();
    else
      Debug.LogError("[BarnBuilding] BarnUI not found in scene!");
  }
}