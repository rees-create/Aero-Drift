using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;


public class InitPlayerPrefabParams : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Vector3 initPos;

    public override void OnNetworkSpawn()
    { 
        gameObject.transform.position = initPos;
    }
    public override void OnNetworkDespawn()
    {
        gameObject.transform.position = initPos;
    }

   
}
