using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanHandler : MonoBehaviour
{
    public LayerMask targetLayers;
    public LayerMask obstructionLayers;
    public float minSize = 0.1f;
    public float maxSize = 5f;
    public float growthSpeed = 2f;
    public float pingLifetime;
    public PingInfo prefabToSpawn;

    // Static dictionary to track active pings
    private static Dictionary<Transform, GameObject> activePings = new Dictionary<Transform, GameObject>();

    private void Start()
    {
        transform.localScale = Vector3.one * minSize;
        StartCoroutine(GrowAndDestroy());
    }

    void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        Vector3 origin = transform.position;
        Vector3 target = other.transform.position;

        if (Physics.Linecast(origin, target, obstructionLayers)) return;

        if (activePings.ContainsKey(other.transform))
        {
            if (activePings[other.transform] == null)
            {
                activePings.Remove(other.transform);
            }
            else
            {
                return;
            }
        }

        GameObject ping = Instantiate(prefabToSpawn.gameObject, target, Quaternion.identity);
        PingInfo pingInfo = ping.GetComponent<PingInfo>();
        pingInfo.info.text = other.GetComponent<Item>().GetName();
        pingInfo.lifetime = pingLifetime;
        pingInfo.trackedTarget = other.transform;

        activePings[other.transform] = ping;
    }

    IEnumerator GrowAndDestroy()
    {
        float currentSize = minSize;

        while (currentSize < maxSize)
        {
            currentSize += growthSpeed * Time.deltaTime;
            currentSize = Mathf.Min(currentSize, maxSize);
            transform.localScale = Vector3.one * currentSize;
            yield return null;
        }

        Destroy(gameObject);
    }

    // Called by PingInfo when it self-destructs
    public static void Untrack(Transform target)
    {
        if (activePings.ContainsKey(target))
        {
            activePings.Remove(target);
        }
    }
}
