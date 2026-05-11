using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(FlightControl))]
public class NetworkFlightControl : NetworkBehaviour
{
    public UnityEvent joystickThrow;
    public UnityEvent endThrow;
    [NonSerialized] public NetworkVariable<Vector2> initialThrowImpulse = new NetworkVariable<Vector2>
    (
        Vector2.zero, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    [ServerRpc]
    public void AddImpulseServerRpc()
    { 
        print($"Impulse of {initialThrowImpulse.Value} added");
        //if(GetComponent<FlightControl>().netlagcount == 0)
        GetComponent<Rigidbody2D>().AddForce(initialThrowImpulse.Value, ForceMode2D.Impulse);
        //GetComponent<FlightControl>().SetInitialThrowImpulse(initialThrowImpulse.Value);
        //GetComponent<FlightControl>().SetInitialThrowImpulse(Vector2.zero)
        if (IsOwner) 
        {
            initialThrowImpulse.Value = Vector2.zero;
        }
    }
    

    [ClientRpc]
    public void EndImpulseClientRpc() 
    {
        //GetComponent<Rigidbody2D>().AddForce(initialThrowImpulse.Value, ForceMode2D.Impulse);
        if (IsOwner)
        {
            initialThrowImpulse.Value = Vector2.zero;
        }
        GetComponent<FlightControl>().SetInitialThrowImpulse(Vector2.zero);
        //GetComponent<FlightControl>().netlagcount = 0;
        //if()
    }

    NetworkVariable<int> ownerLagCount = new NetworkVariable<int>
    (
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    [ServerRpc]
    public void StopLagCountServerRpc()
    {
        if (IsOwner)
        {
            ownerLagCount.Value = 0;
            print("Owner lag value reset");
        }
        
    }

    private void Update()
    {

        if (initialThrowImpulse.Value.magnitude > 0) 
        {
            if (IsOwner)
            {
                if (ownerLagCount.Value == 0)
                {
                    AddImpulseServerRpc();
                }
                ownerLagCount.Value++;
                initialThrowImpulse.Value = Vector2.zero;
            }
            if (IsServer)
            {
                
                EndImpulseClientRpc();
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        //enforce this.
        if (IsServer)
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        }

        joystickThrow.AddListener(() => 
        {
            if (IsOwner)
            {
                AddImpulseServerRpc();
                initialThrowImpulse.Value = Vector2.zero;
            }
        });
        endThrow.AddListener(() =>
        {
            if (IsOwner) 
            {
                StopLagCountServerRpc();
                //ownerLagCount.Value = 0;
                print("Owner lag value would've been reset");
            }
            if (IsServer)
            {
                EndImpulseClientRpc();
            }
        });
    }

}
