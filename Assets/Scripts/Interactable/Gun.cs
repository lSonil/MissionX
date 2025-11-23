using UnityEngine;

public class Gun : Item
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 100f;
    //[SerializeField] private float cooldown = .1f;
    [SerializeField] private int maxAmmo = 6;

    public int currentAmmo = 100;
    private float lastFireTime = -Mathf.Infinity;

    public override void PrimaryUse()
    {

        if (currentAmmo <= 0)
        {
            Debug.Log("Gun No ammo!");
            return;
        }

        lastFireTime = Time.time;
        currentAmmo--;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * bulletSpeed, ForceMode.VelocityChange);
        }

        Debug.Log($"Gun Gun Bang! Ammo left: {currentAmmo}");
    }

    public override void SecundaryUse()
    {
        InventorySystem inv = Object.FindFirstObjectByType<InventorySystem>();
        if (inv == null) return;

        Item ammo = inv.ConsumeFirstMatching(item => item is Ammo);
        if (ammo != null && currentAmmo < maxAmmo)
        {
            currentAmmo += ammo.GetComponent<Ammo>().ammoAmount;
            Debug.Log($"Gun Reloaded. Ammo: {currentAmmo}");
        }
        else
        {
            Debug.Log("Gun No bullets to reload.");
        }
    }
}