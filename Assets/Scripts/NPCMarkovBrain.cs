using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class NPCMarkovBrain : MonoBehaviour
{
    public enum NPCStates { 
        Standing,
        Moving,
        SwitchingLayer,
        ThrowingPlane,
        CatchingPlane,
        DestroyingPlane
    }
    //then make the probability transition matrix
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
