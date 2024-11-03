using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class ObjectSpawnSystem : MonoBehaviour
{
    //Coroutine first renders objects from start to end.
    //On the approach of end objects, plane is spawned back to start point and starting objects are destroyed

    //For popping plane back to scene, record prevForce = <AeroUpdate force + torque> + gravity for last frame,
    //move to start without AeroUpdate call, just applying the previous force. 
    // Start is called before the first frame update

    [SerializeField] GameObject element;
    [SerializeField] ElementVariation elementVariation;

    [Serializable]
    struct ElementVariation 
    {
        public int numberOfObjects;
        public float popBackDistance;
        public float colorSeed;
        public Vector3 spawnSpacing;
        public Vector3 scale;

        float rand(float seed, float index, float numberOfObjects) 
        {
            return ((seed + (seed * Mathf.Pow(2, index))) % numberOfObjects) / numberOfObjects;
        }
        public Vector4 ColorRandomization(float seed, float index, float numberOfObjects)
        {
            return new Vector4(rand(seed, index, numberOfObjects), 
                rand(seed, index + 1, numberOfObjects), 
                rand(seed, index + 2, numberOfObjects), 1);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    IEnumerator SpawnObjects()
    {
        List<GameObject> objectList = new List<GameObject>();
        while (true)
        {
            if (transform.childCount != 0)
            {
                int nDeleted = 0;
                int childCount = transform.childCount;
                print($"Initial child count: {childCount}");
                
                while(transform.childCount > 0)
                {
                    //print($"childIndex = {0}; childCount = {transform.childCount}");
                    Transform child = transform.GetChild(0);
                    child.parent = null;
                    DestroyImmediate(child.gameObject);
                    nDeleted += 1;
                }
                
                print($"nDeleted {nDeleted} childCount {transform.childCount}");
                    //objectList = new List<GameObject>();
            }
            for (int i = 0; i < elementVariation.numberOfObjects; i++)
            {
                GameObject g = Instantiate(element, transform);
                g.name = gameObject.name + "_" + i;
                g.transform.localScale = elementVariation.scale;
                g.transform.position = transform.TransformPoint(elementVariation.spawnSpacing * i);
                Vector4 color = elementVariation.ColorRandomization(elementVariation.colorSeed, i, elementVariation.numberOfObjects);
                //print($"Color of house {i} = {color}");
                g.GetComponent<SpriteRenderer>().color = color;
                
            }
            ElementVariation oldElementVariation = elementVariation;
            yield return new WaitUntil(() => !elementVariation.Equals(oldElementVariation));
        }
    }

    [ExecuteInEditMode]
    void Start()
    {
        StartCoroutine(SpawnObjects());
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
