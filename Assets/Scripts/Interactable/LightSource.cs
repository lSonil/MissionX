using UnityEngine;
using System.Collections;

public class LightSource : MonoBehaviour
{
    [Header("Settings")]
    public bool canHappen = true;
    public Material glitchMaterial;
    public float baseProbability = 0.05f;
    public float rapidProbability = 0.5f;

    [Header("References")]
    public Light childLight;
    public MeshRenderer childMesh;
    public AudioSource childAudio;

    private Material originalMaterial;
    private float currentProbability;
    private int successStreak = 0;
    private bool isRunningSequence = false;
    private float originalIntensity;

    void Start()
    {
        currentProbability = baseProbability;
        if (childLight != null) originalIntensity = childLight.intensity;
        if (childMesh != null && childMesh.materials.Length > 1)
            originalMaterial = childMesh.materials[1];
    }

    void Update()
    {
        if (!canHappen || isRunningSequence) return;

        if (Random.value < currentProbability)
        {
            StartCoroutine(GlitchSequence());
        }
        else
        {
            successStreak = 0;
            currentProbability = baseProbability;
        }
    }

    IEnumerator GlitchSequence()
    {
        isRunningSequence = true;
        successStreak++;

        if (successStreak >= 5)
        {
            ShutdownPermanently();
            yield break;
        }

        currentProbability = rapidProbability;

        if (childAudio) childAudio.Play();

        if (childLight) childLight.intensity = originalIntensity * 0.5f;
        if (childMesh) childMesh.enabled = false;

        yield return new WaitForSeconds(.2f);

        if (childLight) childLight.intensity = originalIntensity;
        if (childMesh)
        {
            childMesh.enabled = true;
            SwapMaterial(glitchMaterial);
        }

        SwapMaterial(originalMaterial);

        isRunningSequence = false;
    }

    void SwapMaterial(Material newMat)
    {
        if (childMesh == null || childMesh.materials.Length < 2) return;

        Material[] mats = childMesh.materials;
        mats[1] = newMat;
        childMesh.materials = mats;
    }

    void ShutdownPermanently()
    {
        if (childLight) childLight.enabled = false;
        if (childAudio) childAudio.Stop();
        SwapMaterial(glitchMaterial);
        canHappen = false;
        Debug.Log("System Permanently Failed.");
    }
}