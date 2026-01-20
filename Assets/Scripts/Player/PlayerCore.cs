using Unity.Netcode;
using UnityEngine;

public class PlayerCore : NetworkBehaviour
{
    [HideInInspector]
    public CharacterController cc;
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
    public GameObject body;
    bool dead;
    Transform focus;
    private void Start()
    {        
        cc = GetComponent<CharacterController>();
        ms = GetComponent<MovementSystem>();
        ss = GetComponent<SanitySystem>();
        ivs = GetComponent<InventorySystem>();
        its = GetComponent<InteractionSystem>();
        hs = GetComponent<HealthSystem>();
        ans = GetComponent<AnimatorSystem>();
        aus = GetComponent<AudioSystem>();
        uis = GetComponent<UISystem>();
    }

    public void Update()
    {
        if (!IsOwner) return;
        if(dead)
        {
            transform.LookAt(focus);
            return;
        }
        ms.NetworkUpdate();
        ans.NetworkUpdate();

    }
    public override void OnNetworkSpawn() {
        if (IsServer) LobyData.Register(this);
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer) LobyData.Unregister(this);
    }
    public void GetReady(Transform pos)
    {
        cc.enabled = true;
        ms.enabled = true;
        ss.enabled = true;
        ivs.enabled = true;
        its.enabled = true;
        hs.enabled = true;
        ans.enabled = true;
        aus.enabled = true;
        uis.enabled = true;
        body.SetActive(true);
        transform.position = pos.position;
        transform.rotation = Quaternion.identity;
        dead = false;
    }
    public void Stop(Transform theFocus,Transform pos = null)
    {
        cc.enabled = false;
        ms.enabled = false;
        ss.enabled = false;
        ivs.enabled = false;
        its.enabled = false;
        hs.enabled = false;
        ans.enabled = false;
        aus.enabled = false;
        uis.enabled = false;

        if(pos != null) transform.position = pos.position;
        focus = theFocus;
        dead = true;
        body.SetActive(false);
    }

}
