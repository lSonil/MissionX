using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.name);

        if (collision.gameObject.GetComponent<Gun>() == null)
            Destroy(gameObject);
    }
}
