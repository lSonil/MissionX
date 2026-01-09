using UnityEngine;

public class Show : MonoBehaviour
{
    [SerializeField] private float speed = 20f; // degrees per second

    void Update()
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f, Space.Self);
    }
}
