using UnityEngine;

public class Road : MonoBehaviour
{
    [Header("District Info")]
    public string districtID = "none";

    private void Start()
    {
        // Ensure the object has the "road" tag as requested
        if (!CompareTag("road"))
        {
            gameObject.tag = "road";
        }
    }
}
