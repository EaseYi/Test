using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkMangerUI : MonoBehaviour
{

    [SerializeField]
    private Button hostBtn;   
    [SerializeField]
    private Button serverBtn; 
    [SerializeField]
    private Button clientBtn; 
    // Start is called before the first frame update
    void Start()
    {
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            DestoryAllButtons();
        });
        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            DestoryAllButtons();
        });
        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            DestoryAllButtons();
        });
    }
    private void DestoryAllButtons()
    {
        Destroy(hostBtn.gameObject);
        Destroy(serverBtn.gameObject);
        Destroy(clientBtn.gameObject);
    }

}
