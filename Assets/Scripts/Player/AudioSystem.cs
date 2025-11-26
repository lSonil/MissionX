using UnityEngine;
using System.Collections.Generic;

public class AudioSystem : MonoBehaviour
{
    [Header("Audio Source for footsteps")]
    public AudioSource footstepSource;
    [Header("Footstep Clips")]
    public List<AudioClip> footstepClips = new List<AudioClip>();

    private int clipIndex = 0;
    private bool playLeftNext = true; // toggle flag

    [Header("Audio Source for Jump")]
    public AudioSource jumpSource;
    [Header("Clips")]
    public AudioClip jump;
    public AudioClip land;

    [Header("Audio Source for Flashlight")]
    public AudioSource flashlight;

    [Header("Audio Source for Health")]
    public AudioSource health;

    public void PlayFlashlight()
    {
        flashlight.Play();
    }
    public void PlayDamage()
    {
        health.Play();
    }
    public void PlayFootstep()
    {
        if (footstepClips.Count == 0 || footstepSource == null) return;

        footstepSource.clip = footstepClips[clipIndex];

        footstepSource.panStereo = playLeftNext ? -0.1f : 0.1f;

        footstepSource.Play();

        clipIndex++;
        if (clipIndex >= footstepClips.Count)
            clipIndex = 0;

        playLeftNext = !playLeftNext;
    }
    public void PlayJumpEffect()
    {
        jumpSource.clip = jump;
        jumpSource.Play();
    }
    public void PlayLandEffect()
    {
        jumpSource.clip = land;
        jumpSource.Play();
    }
}
