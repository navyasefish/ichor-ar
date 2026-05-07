using UnityEngine;

public class DistrictFlag : MonoBehaviour
{
    [Header("District Settings")]
    public string districtID = "district_1";

    private void OnEnable()
    {
        if (DistrictManager.Instance != null)
        {
            DistrictManager.Instance.RegisterDistrictFlag(this);
        }
    }

    private void OnDisable()
    {
        if (DistrictManager.Instance != null)
        {
            DistrictManager.Instance.UnregisterDistrictFlag(this);
        }
    }

    // Helper method to check if visible on camera
    public bool IsVisible(Camera cam)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, GetComponent<Collider>()?.bounds ?? new Bounds(transform.position, Vector3.one));
    }
}
