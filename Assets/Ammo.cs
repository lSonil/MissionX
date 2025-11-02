using UnityEngine;

public class Ammo : Item
{
    public int ammoAmount;
    public override void PrimaryUse()
    {
        Debug.Log("This is ammo. Use it to reload a gun.");
    }

    public override void SecundaryUse()
    {
        Debug.Log("Bullets can't be used directly.");
    }
}
