using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] List<GameObject> elements;
    [SerializeField] GameObject player;
    [SerializeField] Vector3 playerInitialPosition;
    //[SerializeField] GameObject virtualCamera;
    [SerializeField] List<ElementVariation> elementVariations;
    [Header("Pop Back Settings")]
    public float popBackProximity;
    public int popBackAtLast;
    

    bool popBack = false;

    [Serializable]
    class ElementVariation
    {
        public int numberOfObjects;
        [NonSerialized] public Vector3 popBackAtLastPosition;
        public bool triggersPopBack;
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
        int numElements = elements.Count;
        while (true)
        {
            if (elements.Count != 0)
            {
                //elementVariations.Clear();this is the problem
                //add ElementVariations for each element
                if (numElements != elements.Count)
                {
                    int sign = (elements.Count - numElements) / Mathf.Abs(numElements - elements.Count);
                    for (int i = numElements; i != elements.Count; i += sign)
                    {
                        if (i > 0)
                            elementVariations.Add(new ElementVariation());
                        else
                            elementVariations.RemoveAt(elements.Count - 1);
                    }
                    numElements = elements.Count;
                }
                //set each Element according to its elementVariation
                
                //flush objects
                print("flushed?");
                if (popBack)
                {
                    for (int elVarIdx = 0; elVarIdx < elementVariations.Count; elVarIdx++)
                    {
                        elementVariations[elVarIdx].phaseSeed += Mathf.PI;
                    }
                }
                if (transform.childCount != 0 || popBack)
                {
                    int nDeleted = 0;
                    int childCount = transform.childCount;
                    //unparent and destroy all child objects
                    while (transform.childCount > 0)
                    {
                        Transform child = transform.GetChild(0);
                        child.parent = null;
                        DestroyImmediate(child.gameObject);
                        nDeleted += 1;
                    }

                }

                //spawn objects
                for (int elVarIdx = 0; elVarIdx < elementVariations.Count; elVarIdx++)
                {
                    //foreach (GameObject element in elements)
                    //{
                    print("hello?");
                        for (int i = 0; i < elementVariations[elVarIdx].numberOfObjects; i++)
                        {
                            print("is this doing anything");
                            GameObject g = Instantiate(elements[elVarIdx], transform);
                            g.name = gameObject.name + "_" + i;
                            g.transform.localScale = elementVariations[elVarIdx].scale;
                            g.transform.position = transform.TransformPoint(elementVariations[elVarIdx].spawnSpacing * i) + elementVariations[elVarIdx].position;
                            if (player != null && i == elementVariations[elVarIdx].numberOfObjects - popBackAtLast
                                && elementVariations[elVarIdx].triggersPopBack)
                            {
                                elementVariations[elVarIdx].popBackAtLastPosition = g.transform.position;
                            }
                            g.transform.eulerAngles = transform.TransformVector(elementVariations[elVarIdx].rotation);
                            Vector4 color = elementVariations[elVarIdx].ColorRandomization(elementVariations[elVarIdx].phaseSeed, i, elementVariations[elVarIdx].numberOfObjects);
                            //print($"Color of house {i} = {color}");
                            if (elementVariations[elVarIdx].overwriteColor)
                            {
                                g.GetComponent<SpriteRenderer>().color = color;
                            }
                        }
                    //}
                }
            }
            List<ElementVariation> oldElementVariations = elementVariations;
            print(!elementVariations.SequenceEqual(oldElementVariations));
            yield return new WaitUntil(() => popBack || !elementVariations.SequenceEqual(oldElementVariations));
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
        Vector3 popBackAtLastPosition = new Vector3();

        //To optimize performance, put the pop back triggering element in the front of the list!
        foreach(ElementVariation elementVariation in elementVariations) 
        {
            if (elementVariation.triggersPopBack)
                popBackAtLastPosition = elementVariation.popBackAtLastPosition;
                break;
        }
        //pop back
        if (player != null && popBackAtLastPosition.x - player.transform.position.x
            <= popBackProximity) //then pop back
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
