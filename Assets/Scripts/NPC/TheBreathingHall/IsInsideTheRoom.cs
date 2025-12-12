using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsInsideTheRoom : MonoBehaviour
{
    public bool isPlayerInside = false;

    [Header("Sanity Interaction")]
    public float sanityDrainRate = 2f;
    public List<Transform> players;
    public AudioSource asrc;
    public AudioClip[] sounds;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Random.value <= 0.1f)
            {
                if (sounds.Length > 0)
                {
                    int index = Random.Range(0, sounds.Length);
                    asrc.clip = sounds[index];
                    asrc.Play();
                }
            }
            players.Add(other.transform);
            isPlayerInside = true;
            StartCoroutine(Drain(other.GetComponent<SanitySystem>()));
        }
    }
    IEnumerator Drain(SanitySystem ss)
    {
        bool drain = GetComponentInParent<NPCTheBreathingHall>()==null? true:GetComponentInParent<NPCTheBreathingHall>().contained != ContainedState.Contained;
        while (isPlayerInside && drain)
        {
            drain = GetComponentInParent<NPCTheBreathingHall>() == null ? true : GetComponentInParent<NPCTheBreathingHall>().contained != ContainedState.Contained;
            ss.DrainSanity(sanityDrainRate);
            yield return new WaitForSeconds(1f);

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            players.Remove(other.transform);
            isPlayerInside = false;
        }
    }
}
