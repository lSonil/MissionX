using UnityEngine;

public class IsInsideTheWalls : MonoBehaviour
{
    private int playerTriggerCount = 0;
    public bool isPlayerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTriggerCount++;
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTriggerCount = Mathf.Max(0, playerTriggerCount - 1);
            if (playerTriggerCount == 0)
            {
                isPlayerInside = false;
            }
        }
    }
}
