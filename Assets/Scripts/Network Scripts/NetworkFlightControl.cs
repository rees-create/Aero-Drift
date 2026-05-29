using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(FlightControl))]
public class NetworkFlightControl : NetworkBehaviour
{
    [NonSerialized] public UnityEvent joystickThrow;
    [NonSerialized] public UnityEvent endThrow;
    [NonSerialized] public NetworkVariable<Vector2> initialThrowImpulse = new NetworkVariable<Vector2>
    (
        Vector2.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    NetworkVariable<int> ownerLagCount = new NetworkVariable<int>
    (
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    NetworkVariable<float> thrust = new NetworkVariable<float>
    (
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    //public bool thrustOverNetwork;

    [ServerRpc]
    public void AddImpulseServerRpc()
    {
        print($"Impulse of {initialThrowImpulse.Value} added");
        //if(GetComponent<FlightControl>().netlagcount == 0)
        GetComponent<Rigidbody2D>().gravityScale = 1;
        GetComponent<Rigidbody2D>().AddForce(initialThrowImpulse.Value, ForceMode2D.Impulse);
        //GetComponent<FlightControl>().SetInitialThrowImpulse(initialThrowImpulse.Value);
        //GetComponent<FlightControl>().SetInitialThrowImpulse(Vector2.zero)

        //if (IsOwner) 
        //{
        //    initialThrowImpulse.Value = Vector2.zero;
        //}
    }

    public bool serverAuthority;


    [ClientRpc]
    public void EndImpulseClientRpc() 
    {
        //GetComponent<Rigidbody2D>().AddForce(initialThrowImpulse.Value, ForceMode2D.Impulse);
        if (IsOwner)
        {
            initialThrowImpulse.Value = Vector2.zero;
            print("Ended initial throw impulse on server");
        }
        GetComponent<Rigidbody2D>().gravityScale = 1;
        GetComponent<FlightControl>().SetInitialThrowImpulse(Vector2.zero);
        //GetComponent<FlightControl>().netlagcount = 0;
        //if()
    }

    

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

        if (initialThrowImpulse.Value.magnitude > 0.001) 
        {
            if (IsOwner)
            {
                if (ownerLagCount.Value == 0)
                {
                    if (!IsServer)
                    {
                        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic; //--after save: was kinematic
                    }
                    AddImpulseServerRpc();
                }
                else 
                {
                    StopLagCountServerRpc();
                }
                ownerLagCount.Value++;
                
            }
            if (IsServer)
            {
                if (ownerLagCount.Value > 0)
                {
                    EndImpulseClientRpc();
                }
            }
        }
    }
    Rigidbody2D rb;
    
    private void FixedUpdate()
    {
        //if (thrustOverNetwork) 
        //{

        //}
        //if (GetComponent<FlightControl>().thrust > 0)
        //{
        //    print(GetComponent<FlightControl>().thrust);
        //}
    }

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        //enforce this.
        if (IsServer)
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            
        }
    }

}
