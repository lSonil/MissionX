using UnityEngine;

public class Data : Item
{
    public int offset =  20;
    int actualOffset;

    private void Start()
    {
        actualOffset = Random.Range(-offset, offset + 1);
    }

    public override int GetWeight()
    {
        return itemWeight + itemWeight * actualOffset / 100;
    }

    public override void PrimaryUse()
    {
        Debug.Log("This is info, bring it back.");
    }

    public override void SecundaryUse()
    {
        Debug.Log("This is info, bring it back.");
    }
}
