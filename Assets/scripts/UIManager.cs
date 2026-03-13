using UnityEngine;

public class UIManager : MonoBehaviour
{
  public GameObject homePanel;
  public GameObject categoryPanel;
  public GameObject itemPanel;
  public GameObject placementPanel;
  public GameObject scanPanel;

  void Start()
  {
    ShowHome();
  }

  public void ShowHome()
  {
    homePanel.SetActive(true);
    categoryPanel.SetActive(false);
    itemPanel.SetActive(false);
    placementPanel.SetActive(false);
    scanPanel.SetActive(false);
  }

  public void ShowCategories()
  {
    homePanel.SetActive(false);
    categoryPanel.SetActive(true);
  }

  public void ShowPlacement()
  {
    categoryPanel.SetActive(false);
    placementPanel.SetActive(true);
  }

  public void BackToCategories()
  {
    placementPanel.SetActive(false);
    categoryPanel.SetActive(true);
  }
}