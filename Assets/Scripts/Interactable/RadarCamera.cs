using UnityEngine;

public class RadarCamera : MonoBehaviour
{
    [SerializeField] Vector3 targetEuler = new Vector3(90f, 00f, 0);
    Quaternion targetRotation;

    void Awake()
    {
        targetRotation = Quaternion.Euler(targetEuler);
    }

    void LateUpdate()
    {
        transform.rotation = targetRotation;
    }
}
