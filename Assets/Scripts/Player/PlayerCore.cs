using UnityEngine;

public class PlayerCore : MonoBehaviour
{
    [HideInInspector]
    public MovementSystem ms;
    [HideInInspector]
    public SanitySystem ss;
    [HideInInspector]
    public InventorySystem ivs;
    [HideInInspector]
    public InteractionSystem its;
    [HideInInspector]
    public HealthSystem hs;
    [HideInInspector]
    public AnimatorSystem ans;
    [HideInInspector]
    public AudioSystem aus;
    [HideInInspector]
    public UISystem uis;

    private void Start()
    {
        ms = GetComponent<MovementSystem>();
        ss = GetComponent<SanitySystem>();
        ivs = GetComponent<InventorySystem>();
        its = GetComponent<InteractionSystem>();
        hs = GetComponent<HealthSystem>();
        ans = GetComponent<AnimatorSystem>();
        aus = GetComponent<AudioSystem>();
        uis = GetComponent<UISystem>();
    }
}
