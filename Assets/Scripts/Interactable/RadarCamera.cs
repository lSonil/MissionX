using UnityEngine;

public class RadarCamera : MonoBehaviour
{
    void LateUpdate()
    {
        // Force global rotation to (90, 0, 0)
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
