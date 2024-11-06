using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
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
    [SerializeField] GameObject player;
    [SerializeField] Vector3 playerInitialPosition;
    [SerializeField] GameObject virtualCamera;
    [SerializeField] ElementVariation elementVariation;
    bool popBack = false;

    [Serializable]
    struct ElementVariation
    {
        public int numberOfObjects;
        [Header("Pop Back Settings")]
        public float popBackProximity;
        public int popBackAtLast;
        [NonSerialized] public Vector3 popBackAtLastPosition;
        [Header("Wave \"Randomization\" Properties")]
        public float phaseSeed;
        public float randomizationCycleFrequency;
        [Header("Color Properties")]
        public bool overwriteColor;
        [Header("Transform Properties")]
        public Vector3 spawnSpacing;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        

        float rand(float seed, float index, float numberOfObjects) 
        {
            float frequency = randomizationCycleFrequency;
            float divisor = numberOfObjects != 0 ? numberOfObjects : 1;
            float waveValue = Mathf.Abs(Mathf.Sin(Mathf.PI * frequency * (index / divisor)) + 
                Mathf.Sin((Mathf.PI * frequency * index) + seed))
                % 1;
            //return ((seed + (seed * Mathf.Pow(2, index))) % numberOfObjects) / numberOfObjects;
            return waveValue;
        }
        public Vector4 ColorRandomization(float seed, float index, float numberOfObjects)
        {
            return new Vector4(rand(seed, index, numberOfObjects), 
                rand(seed, index + seed, numberOfObjects), 
                rand(seed, index + seed * 2, numberOfObjects), 1);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    IEnumerator SpawnObjects()
    {
       
        while (true)
        {
            if (popBack)
            {
                elementVariation.phaseSeed += Mathf.PI;
            }
            if (transform.childCount != 0 || popBack)
            {
                int nDeleted = 0;
                int childCount = transform.childCount;
                
                while(transform.childCount > 0)
                {
                    Transform child = transform.GetChild(0);
                    child.parent = null;
                    DestroyImmediate(child.gameObject);
                    nDeleted += 1;
                }
                
            }
            //spawn objects
            for (int i = 0; i < elementVariation.numberOfObjects; i++)
            {
                GameObject g = Instantiate(element, transform);
                g.name = gameObject.name + "_" + i;
                g.transform.localScale = elementVariation.scale;
                g.transform.position = transform.TransformPoint(elementVariation.spawnSpacing * i) + elementVariation.position;
                if (player != null && i == elementVariation.numberOfObjects - elementVariation.popBackAtLast) 
                {
                    elementVariation.popBackAtLastPosition = g.transform.position;
                }
                g.transform.eulerAngles = transform.TransformVector(elementVariation.rotation);
                Vector4 color = elementVariation.ColorRandomization(elementVariation.phaseSeed, i, elementVariation.numberOfObjects);
                //print($"Color of house {i} = {color}");
                if (elementVariation.overwriteColor)
                {
                    g.GetComponent<SpriteRenderer>().color = color;
                }
            }
            ElementVariation oldElementVariation = elementVariation;
            yield return new WaitUntil(() => popBack || !elementVariation.Equals(oldElementVariation));
        }
    }

    [ExecuteInEditMode]
    void Start()
    {
        if (player != null)
        {
            playerInitialPosition = player.transform.position;
        }
        StartCoroutine(SpawnObjects());
    }

    // Update is called once per frame
    void Update()
    {
        popBack = false;
        if (player != null && elementVariation.popBackAtLastPosition.x - player.transform.position.x
            <= elementVariation.popBackProximity) //then pop back
        {
            popBack = true;
            Vector3 velocity = player.GetComponent<Rigidbody2D>().velocity;
            player.GetComponent<FlightControl>().enabled = false;
            Vector3 newPosition = player.transform.position;
            newPosition.x = playerInitialPosition.x;
            player.transform.position = newPosition;
            player.GetComponent<Rigidbody2D>().velocity = velocity;
            player.GetComponent<FlightControl>().enabled = true;
        }
    }
}
