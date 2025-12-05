using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    public GameObject lobbyTerminal;
    public GameObject missionTerminal;
    public Transform spawnPoint;
    public Transform playerSpawnPoint;
    public Transform player;
    public Transform playerPrefab;

    private void Awake()
    {
        // Check if another instance already exists
        Lobby[] instances = Object.FindObjectsByType<Lobby>(FindObjectsSortMode.None);

        if (instances.Length > 1)
        {
            // If this is not the first one, destroy it
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ToggleTerminals(scene.name);
    }

    private void ToggleTerminals(string sceneName)
    {
        if (sceneName == "Mission")
        {
            Instantiate(missionTerminal, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Instantiate(lobbyTerminal, spawnPoint.position, spawnPoint.rotation);
            if (player == null)
                player = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }

}
