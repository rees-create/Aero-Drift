using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FloorControl : MonoBehaviour
{
    public struct LayerInfo { 
        public string levelName;
        public int layerLevel;
        public float floorY;
    }

    public List<LayerInfo> layers;
    public int FloorLevel
    {
        get { return layerLevel; }
        set { layerLevel = value; }
    }
    
    public float FloorDepth
    {
        get { return floorY; }
        set { floorY = value; }
    }
    
    public int NumberOfFloors
    {
        get { return numberOfFloors; }
        set { numberOfFloors = value; }
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
