using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;
using UnityEngine.SceneManagement;

public class ConnectUI : MonoBehaviour
{
    [SerializeField] private Button host;
    [SerializeField] private Button client;
    [SerializeField] private Button quit; // Added Third Button

    void Start()
    {
        host.onClick.AddListener(HostButtonOnClick);
        client.onClick.AddListener(ClientButtonOnClick);

        // Register the quit listener
        if (quit != null)
        {
            quit.onClick.AddListener(QuitButtonOnClick);
        }
    }

    void HostButtonOnClick()
    {
        StartCoroutine(HostFlow());
    }

    private IEnumerator HostFlow()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Single);
        while (!op.isDone) { yield return null; }
        NetworkManager.Singleton.StartHost();
    }

    void ClientButtonOnClick()
    {
        NetworkManager.Singleton.StartClient();
    }

    // New method to handle quitting
    void QuitButtonOnClick()
    {
        Debug.Log("Quit Game Requested");

#if UNITY_EDITOR
        // This allows the quit button to work inside the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // This closes the actual build
            Application.Quit();
#endif
    }
    private float escHoldTimer = 0f;
    private float requiredHoldTime = 5f;

    void Update()
    {
        // ... your existing inventory code ...

        // ESC Hold to Reset Game
        if (Input.GetKey(KeyCode.Escape))
        {
            escHoldTimer += Time.deltaTime;

            if (escHoldTimer >= requiredHoldTime)
            {
                ResetFullGame();
            }
        }
        else
        {
            escHoldTimer = 0f;
        }
    }

    void ResetFullGame()
    {
        // 1. Shut down Netcode for GameObjects
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // 2. Clear the hold timer to prevent double-execution
        escHoldTimer = 0f;

        // 3. Load your very first scene (Index 0 in Build Settings)
        // Replace "MainMNenu" with the actual name of your starting scene
        SceneManager.LoadScene(0);
    }
}