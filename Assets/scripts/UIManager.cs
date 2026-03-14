using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
  [Header("Core Panels")]
  public GameObject homePanel;
  public GameObject categoryPanel;
  public GameObject scanPanel;
  public GameObject placementPanel;        // Confirm / Cancel / Rotate — buildings
  public GameObject terrainPlacementPanel; // Confirm / Cancel only — terrain

  // -------------------------------------------------------------------
  // No string-based category mapping needed anymore.
  // Each category button's OnClick calls OnCategorySelected(GameObject)
  // and you drag the matching item panel into the argument slot.
  // -------------------------------------------------------------------

  private GameObject pendingItemPanel = null;  // set when category tapped
  private GameObject activeItemPanel = null;  // set once scan completes

  private GameObject currentPanel;
  private Stack<GameObject> panelHistory = new Stack<GameObject>();

  void Start()
  {
    ShowPanel(homePanel);
  }

  // -------------------------------------------------------------------
  // Assign to every category button OnClick.
  // Drag the item panel you want shown after scanning into the argument.
  //   Housing button  → OnCategorySelected( housingPanel )
  //   Terrain button  → OnCategorySelected( terrainPanel )
  // -------------------------------------------------------------------
  public void OnCategorySelected(GameObject itemPanel)
  {
    if (itemPanel == null)
    {
      Debug.LogError("[UIManager] OnCategorySelected called with a null panel!");
      return;
    }

    pendingItemPanel = itemPanel;
    Debug.Log($"[UIManager] Category selected → pending panel: {itemPanel.name}");

    ShowPanel(scanPanel);

    SurfaceSelectionManager ssm = FindObjectOfType<SurfaceSelectionManager>();
    if (ssm != null)
      ssm.StartScanning();
    else
      Debug.LogError("[UIManager] SurfaceSelectionManager not found in scene!");
  }

  // -------------------------------------------------------------------
  // Called by SurfaceSelectionManager once the grid has been placed.
  // Opens whichever item panel the category button registered.
  // -------------------------------------------------------------------
  public void OnScanComplete()
  {
    if (pendingItemPanel == null)
    {
      Debug.LogError("[UIManager] OnScanComplete — pendingItemPanel is null!");
      return;
    }

    activeItemPanel = pendingItemPanel;
    pendingItemPanel = null;

    Debug.Log($"[UIManager] Scan complete → showing: {activeItemPanel.name}");
    ShowPanel(activeItemPanel);
  }

  // -------------------------------------------------------------------
  // Called by BuildingPlacementManager when a building first hits the grid.
  // Shows Confirm / Cancel / Rotate panel.
  // -------------------------------------------------------------------
  public void OnItemSelected()
  {
    Debug.Log("[UIManager] Building on grid → showing placement panel.");
    ShowPanel(placementPanel);
  }

  // -------------------------------------------------------------------
  // Called by BuildingPlacementManager when terrain mode first hits the grid.
  // Shows Confirm / Cancel only panel (no rotate for terrain).
  // -------------------------------------------------------------------
  public void OnTerrainSelected()
  {
    Debug.Log("[UIManager] Terrain on grid → showing terrain placement panel.");
    ShowPanel(terrainPlacementPanel);
  }

  // -------------------------------------------------------------------
  // Back button on item panels — cancels any active placement and
  // returns to the category panel.
  // -------------------------------------------------------------------
  public void OnBackFromItemPanel()
  {
    Debug.Log("[UIManager] Back from item panel → cancelling placement, going to category.");

    BuildingPlacementManager bpm = FindObjectOfType<BuildingPlacementManager>();
    if (bpm != null) bpm.CancelPlacement();

    activeItemPanel = null;
    ShowPanelDirect(categoryPanel);
  }

  // -------------------------------------------------------------------
  // Called by BuildingPlacementManager after ConfirmPlacement()
  // or CancelPlacement() finishes.
  // Returns the player cleanly to the active item panel.
  // -------------------------------------------------------------------
  public void OnPlacementFinished()
  {
    if (activeItemPanel == null)
    {
      Debug.LogWarning("[UIManager] OnPlacementFinished — no activeItemPanel, falling back to GoBack.");
      GoBack();
      return;
    }

    Debug.Log($"[UIManager] Placement finished → returning to: {activeItemPanel.name}");
    ShowPanelDirect(activeItemPanel);
  }

  // -------------------------------------------------------------------
  // Generic navigation
  // -------------------------------------------------------------------
  public void ShowPanel(GameObject panel)
  {
    if (panel == null)
    {
      Debug.LogError("[UIManager] ShowPanel — panel is null!");
      return;
    }

    Debug.Log("[UIManager] Opening panel: " + panel.name);

    if (currentPanel != null)
    {
      panelHistory.Push(currentPanel);
      currentPanel.SetActive(false);
    }

    panel.SetActive(true);
    currentPanel = panel;
  }

  public void GoBack()
  {
    if (panelHistory.Count == 0)
      return;

    currentPanel.SetActive(false);

    GameObject previous = panelHistory.Pop();
    previous.SetActive(true);
    currentPanel = previous;
  }

  // Jump directly to a panel without pushing history.
  // Used for clean returns (e.g. after confirm/cancel placement).
  private void ShowPanelDirect(GameObject panel)
  {
    if (currentPanel != null)
      currentPanel.SetActive(false);

    panelHistory.Clear(); // prevent stale back-navigation
    panel.SetActive(true);
    currentPanel = panel;
  }
}