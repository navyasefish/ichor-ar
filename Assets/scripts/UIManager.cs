using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
  public GameObject homePanel;
  public GameObject categoryPanel;
  public GameObject itemPanel;
  public GameObject placementPanel;
  public GameObject scanPanel;

  private GameObject currentPanel;
  private Stack<GameObject> panelHistory = new Stack<GameObject>();

  void Start()
  {
    ShowPanel(homePanel);
  }

  public void ShowPanel(GameObject panel)
  {
    Debug.Log("Opening panel: " + panel.name);

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
}