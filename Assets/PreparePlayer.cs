using UnityEngine;

public class PreparePlayer : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = false;

            player.transform.position = transform.position;

            if (controller != null)
                controller.enabled = true;
        }
        else
        {
            Debug.LogWarning("Player object not found!");
        }
    }
}
