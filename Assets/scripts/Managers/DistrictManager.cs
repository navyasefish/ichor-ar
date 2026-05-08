using System.Collections.Generic;
using UnityEngine;

public class DistrictManager : MonoBehaviour
{
    public static DistrictManager Instance;

    [Header("District Settings")]
    public string currentDistrictID = "global";
    public float districtCheckInterval = 0.5f;

    private List<DistrictFlag> activeFlags = new List<DistrictFlag>();
    private float nextCheckTime;
    private TMPro.TextMeshProUGUI testText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            FindTestText();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void FindTestText()
    {
        GameObject testObj = GameObject.Find("Test");
        if (testObj != null)
        {
            testText = testObj.GetComponent<TMPro.TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            UpdateCurrentDistrict();
            nextCheckTime = Time.time + districtCheckInterval;
        }
    }

    public void RegisterDistrictFlag(DistrictFlag flag)
    {
        if (!activeFlags.Contains(flag))
        {
            activeFlags.Add(flag);
        }
    }

    public void UnregisterDistrictFlag(DistrictFlag flag)
    {
        if (activeFlags.Contains(flag))
        {
            activeFlags.Remove(flag);
        }
    }

    public DistrictFlag GetFlagByID(string dID)
    {
        return activeFlags.Find(f => f.districtID == dID);
    }

    private void UpdateCurrentDistrict()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        DistrictFlag closestFlag = null;
        float minDistance = float.MaxValue;

        foreach (var flag in activeFlags)
        {
            if (flag.IsVisible(cam))
            {
                float distance = Vector3.Distance(cam.transform.position, flag.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestFlag = flag;
                }
            }
        }

        if (closestFlag != null)
        {
            if (currentDistrictID != closestFlag.districtID)
            {
                currentDistrictID = closestFlag.districtID;
                DevTools.Log($"District changed to: {currentDistrictID}");
            }
        }

        // Update the 'Test' TMP display
        if (testText != null)
        {
            if (DevTools.Instance != null && !DevTools.Instance.debugMode)
            {
                testText.text = currentDistrictID;
            }
            // If debugMode is on, we let DevTools handle the status text
        }
        else
        {
            // Try to find it again if it was missing (e.g. scene change)
            FindTestText();
        }
    }
}
