using Unity.Netcode;
using UnityEngine;

public class PlayerCore : NetworkBehaviour
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
    public GameObject body;
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

    public void Update()
    {
        if (!IsOwner) return;
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
    }
    public void Stop(Transform focus,Transform pos = null)
    {
        ms.enabled = false;
        ss.enabled = false;
        ivs.enabled = false;
        its.enabled = false;
        hs.enabled = false;
        ans.enabled = false;
        aus.enabled = false;
        uis.enabled = false;

        if(pos != null) transform.position = pos.position;
        transform.LookAt(focus);
        body.SetActive(false);
    }

}
