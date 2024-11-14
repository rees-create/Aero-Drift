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
        //[Header("Animation")]
        //public Animator animator;
        //public bool animateOnTrigger;
        //public TransformAnimation transformAnimations;
        [NonSerialized] public Vector3 popBackAtLastPosition;
        [Header("Wave \"Randomization\" Properties")]
        public float phaseSeed;
        public float randomizationCycleFrequency;
        public Randomizable overwriteProperties;
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
        public Vector4 RandomVector4(float seed, float index, float numberOfObjects)
        {
            return new Vector4(rand(seed, index, numberOfObjects), 
                rand(seed, index + seed, numberOfObjects), 
                rand(seed, index + seed * 2, numberOfObjects), 1);
        }
        public Vector3 RandomVector3(float seed, float index, float numberOfObjects)
        {
            return new Vector3(rand(seed, index, numberOfObjects),
                rand(seed, index + seed, numberOfObjects),
                rand(seed, index + seed * 2, numberOfObjects)
                );
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    //[Serializable]
    //public struct TransformAnimation 
    //{

    //}

    [Serializable]
    public enum OverwriteType { This, Object, Both }
    [Serializable]
    struct Randomizable
    {
        
        public OverwriteType type;
        public bool spawnSpacing;
        public bool color;
        public bool position;
        public bool rotation;
        public bool scale;
    }
    Vector3 multiplyVectors(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
    Vector3 divideVectors(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
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
                //initialize overwrite conditions
                bool overwriteBoth = elementVariation.overwriteProperties.type == OverwriteType.Both;
                bool overwriteThis = elementVariation.overwriteProperties.type == OverwriteType.This || overwriteBoth;
                bool overwriteObject = elementVariation.overwriteProperties.type == OverwriteType.Object || overwriteBoth;
                //instantiate and get to work
                GameObject g = Instantiate(element, transform);
                //check and change overwrite type of object
                if (overwriteObject)
                {
                    OverwriteType type = g.GetComponent<ObjectSpawnSystem>().elementVariation.overwriteProperties.type;
                    if (type == OverwriteType.Object && g.GetComponent<ObjectSpawnSystem>() != null) //make object randomizable
                    {
                        g.GetComponent<ObjectSpawnSystem>().elementVariation.overwriteProperties.type = OverwriteType.Both;
                    }
                    else if(g.GetComponent<ObjectSpawnSystem>() != null) 
                    {
                        print("Your object doesn't have an ObjectSpawnSystem");
                    }
                }
               
                //set name
                g.name = gameObject.name + "_" + i;
                //get scale
                Vector3 scale = elementVariation.scale;
                //potentially randomize scale
                if (elementVariation.overwriteProperties.scale && overwriteThis)
                {
                    scale = multiplyVectors(elementVariation.scale, elementVariation.RandomVector3(elementVariation.phaseSeed, i, elementVariation.numberOfObjects));
                }
                //set scale
                g.transform.localScale = scale;
                //get position
                Vector3 position = elementVariation.position;
                //potentially randomize position
                if (elementVariation.overwriteProperties.position && overwriteThis)
                {
                    position = multiplyVectors(position, elementVariation.RandomVector3(elementVariation.phaseSeed, i, elementVariation.numberOfObjects));
                }
                //potentially randomize spawnSpacing
                Vector3 spawnSpacing = elementVariation.spawnSpacing;
                if (elementVariation.overwriteProperties.spawnSpacing && overwriteThis)
                {
                    spawnSpacing = multiplyVectors(spawnSpacing, elementVariation.RandomVector3(elementVariation.phaseSeed, i, elementVariation.numberOfObjects));
                }
                //set position
                g.transform.position = transform.TransformPoint(spawnSpacing * i) + position;
                //set pop back position
                if (player != null && i == elementVariation.numberOfObjects - elementVariation.popBackAtLast) 
                {
                    elementVariation.popBackAtLastPosition = g.transform.position;
                }
                //get rotation
                Vector3 rotation = transform.TransformVector(elementVariation.rotation);
                //potentially randomize rotation
                if (elementVariation.overwriteProperties.rotation && overwriteThis)
                {
                    rotation += multiplyVectors(rotation, elementVariation.RandomVector3(elementVariation.phaseSeed, i, elementVariation.numberOfObjects));
                }
                //set rotation
                g.transform.eulerAngles = rotation;
                
                //calculate potentially random color
                Vector4 color = elementVariation.RandomVector4(elementVariation.phaseSeed, i, elementVariation.numberOfObjects);
                //potentially set random color
                if (elementVariation.overwriteProperties.color && overwriteThis)
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
