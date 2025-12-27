using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;
using UnityEngine.SceneManagement;
public class ConnectUI : MonoBehaviour
{
    [SerializeField] private Button host;
    [SerializeField] private Button client;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        host.onClick.AddListener(HostButtonOnClick);
        client.onClick.AddListener(ClientButtonOnClick);
    }

    // Update is called once per frame
    void HostButtonOnClick()
    {
        StartCoroutine(HostFlow());
    }
    private IEnumerator HostFlow()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Single);
        while (!op.isDone){
            print(1); yield return null; }
        NetworkManager.Singleton.StartHost();
    }
    void ClientButtonOnClick()
    {
        NetworkManager.Singleton.StartClient();
    }
}