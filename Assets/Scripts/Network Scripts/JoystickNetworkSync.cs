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

    

    private void Update()
    {
        if (GetComponent<FlightJoystick>().flightParams.throwImpulse.magnitude > 0)
        {
            //AddImpulseServerRpc();

        }
    }

    //[ClientRpc]
    public void AddImpulse() 
    {
        if(IsOwner)
            AddImpulseServerRpc();
    }
    [ServerRpc]
    public void AddImpulseServerRpc()
    {
        Vector2 impulse = GetComponent<FlightJoystick>().flightParams.throwImpulse;
        print($"Impulse of {impulse} added");
        GetComponent<FlightJoystick>().flightControl.SetInitialThrowImpulse(impulse);
    }
}
