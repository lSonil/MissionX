using UnityEngine;
using UnityEngine.UI;

public class SanityUI : MonoBehaviour
{
    [SerializeField] private Slider sanitySlider;
    [SerializeField] private SanitySystem playerSanity;

    private void Start()
    {
        if (playerSanity == null)
        {
            playerSanity = FindAnyObjectByType<SanitySystem>();
        }

        if (sanitySlider == null)
        {
            Debug.LogError("SanityUI: Slider not assigned!");
            return;
        }

        playerSanity.OnSanityChanged.AddListener(UpdateBar);
        sanitySlider.maxValue = playerSanity.maxSanity;
        sanitySlider.value = playerSanity.currentSanity;
    }

    private void UpdateBar(float current, float max)
    {
        sanitySlider.value = current;
    }
}
