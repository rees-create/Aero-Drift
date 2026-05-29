using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Unity.Netcode;
using UnityEngine;
public class JoystickNetworkSync : NetworkBehaviour
{
    public UnityEvent startJoystick;
    
    //public override void OnNetworkSpawn()
    //{
    //   startJoystick.Invoke();

    //}
    //NetworkVariable<float> impulseX = new NetworkVariable<float>();
    //NetworkVariable<float> impulseY = new NetworkVariable<float>();

    public override void OnNetworkSpawn()
    {
        if (IsServer) 
        {
            //GetComponent<NetworkObject>().ChangeOwnership();
        }
        //GetComponent<NetworkObject>().ChangeOwnership(GetComponent<NetworkObject>().NetworkObjectId);
        gameObject.name += " " + GetComponent<NetworkObject>().NetworkObjectId;
        GetComponent<FlightJoystick>().isOwner = IsOwner;
        print($"Joystick is owner?: {GetComponent<FlightJoystick>().isOwner} plane name: {GetComponent<FlightJoystick>().flightControl.gameObject.name}");
        
    }

    [ServerRpc]
    void JoystickIdServerRpc() 
    {
        
    }

    
    
}
