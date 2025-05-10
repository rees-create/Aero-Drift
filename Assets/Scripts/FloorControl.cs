using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FloorControl : MonoBehaviour
{
    [System.Serializable]
    public struct LayerInfo { 
        public string levelName;
        public int layerLevel;
        public float floorY;
    }

    public List<LayerInfo> layers;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W)) //move floor up, prime current layer colliders for sorting order reassignment
        {

        }
        if (Input.GetKey(KeyCode.S)) //move floor down, prime lower layer colliders for sorting order reassignment
        {

        }
        //when ready reassign sorting order
    }
}
