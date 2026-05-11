using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;
    [SerializeField] TextMeshProUGUI playersCountText;

    NetworkVariable<int> numPlayers = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);

    private void Awake()
    {
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });
        clientButton.onClick.AddListener(() => 
        {
            NetworkManager.Singleton.StartClient(); 
        });
    }
    public override void OnNetworkSpawn()
    {
        playersCountText.text = $"Players: {numPlayers.Value.ToString()}";
        if (!IsServer) return;
        numPlayers.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }

}
