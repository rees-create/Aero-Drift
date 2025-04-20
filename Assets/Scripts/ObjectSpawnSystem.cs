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

    [SerializeField] GameObject element;
    [SerializeField] GameObject player;
    [SerializeField] GameObject popBackController;
    [SerializeField] Vector3 playerInitialPosition;
    int numAnimResetRequests = 0;
    //[SerializeField] GameObject depthController;
    public ElementVariation elementVariation;
    [NonSerialized] public bool popBack = false;

    public void CountResetRequest()
    {
        if (numAnimResetRequests == elementVariation.numberOfObjects) 
        {
            numAnimResetRequests = 0;
        }
        print($"numAnimResetRequests: {numAnimResetRequests}");
        numAnimResetRequests++;
    }

    [Serializable]
    public struct ElementVariation
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
        [NonSerialized] public Transform popBackTransform;
        [Header("Wave \"Randomization\" Properties")]
        public float phaseSeed;
        public float randomizationCycleFrequency;
        public float seedJump;
        public Randomizable overwriteProperties;
        [NonSerialized] public int iterator;
        [Header("Reference Frame Object?")]
        public bool inReferenceFrame;
        [Header("Layer Position")]
        public int layerPosition;
        [Header("Transform Properties")]
        public Vector3 spawnSpacing;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        /// <summary>
        /// Object Spawn System randomization function. Based on a combination of 2 sine waves, it returns a value between 0 and 1.
        /// </summary>
        /// <param name="seed">seed from elementVariation</param>
        /// <param name="index">index of object being randomized</param>
        /// <param name="numberOfObjects"> useless param. gotta get rid of it but im too lazy to</param>
        /// <returns></returns>
        public float rand(float seed, float index, float numberOfObjects) 
        {
            float frequency = randomizationCycleFrequency;
            float divisor = seed != 0 ? seed : 1;
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
    //private void ResetBlueShift(Color randomColor, ref GameObject gameObject)
    //{
    //    if (gameObject.GetComponent<BlueShift>() != null) 
    //    {
    //        gameObject.GetComponent<SpriteRenderer>().color = gameObject.GetComponent<BlueShift>().
    //    }
    //}

    [Serializable]
    public enum OverwriteType { This, Object, Downward, Parent }
    [Serializable]
    public enum NumberOfObjectsType {Random, Limiting}
    [Serializable] public struct NumberOfObjectsSelector 
    {
        public bool on;
        public NumberOfObjectsType type;
        public float limitingDecider;
    }
    [Serializable]  
    public enum ColorSelector {Random, RandomChannel, RandomTone }
    [Serializable]
    public struct Randomizable
    {
        
        public OverwriteType type;
        public bool randomizeChild;
        public int childIndex;
        public NumberOfObjectsSelector numberOfObjects;
        public bool spawnSpacing;
        public bool color;
        public bool position;
        public bool rotation;
        public bool scale;
        public bool layerPosition;
        public bool other; 
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
                elementVariation.phaseSeed += elementVariation.seedJump;
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
            //AnimationRandomizer minAnim = default;
            //float minAnimDuration = 0;
            //int minAnimDurationIndex = 0;
            //spawn objects
            for (int i = 0; i < elementVariation.numberOfObjects; i++)
            {
                
                //initialize overwrite conditions
                bool parentOverwrite = elementVariation.overwriteProperties.type == OverwriteType.Parent;
                bool overwriteDownward = elementVariation.overwriteProperties.type == OverwriteType.Downward;
                bool overwriteThis = elementVariation.overwriteProperties.type == OverwriteType.This || overwriteDownward;
                bool overwriteObject = elementVariation.overwriteProperties.type == OverwriteType.Object || overwriteDownward;
                //set iterator (it gets overwritten if overwrite condition is Parent
                elementVariation.iterator = i;
                //check and change overwrite type of object
                if (parentOverwrite && transform.parent != null) 
                {
                    elementVariation.iterator = gameObject.name.Last() - '0';
                    //print($"iterator value = {elementVariation.iterator}, last of name = {gameObject.name.Last() - '0'}");
                }
                if (overwriteObject)
                {
                    if (element.GetComponent<ObjectSpawnSystem>() == null)
                    {
                        print("Your object doesn't have an ObjectSpawnSystem");
                    }
                    else
                    {
                        OverwriteType type = element.GetComponent<ObjectSpawnSystem>().elementVariation.overwriteProperties.type;
                        if (type == OverwriteType.Object && element.GetComponent<ObjectSpawnSystem>() != null) //make object randomizable
                        { //oh no triple nested if.. well it's clear what we're doing here, right?
                            element.GetComponent<ObjectSpawnSystem>().elementVariation.overwriteProperties.type = OverwriteType.Parent;
                        }
                    }
                    
                }
                //if (overwriteDownward) downward is currently unconfigured
                //{
                    
                //}
                //first decide whether to instantiate
                
                bool shouldISpawn = true;
                GameObject g = element;
                if (elementVariation.overwriteProperties.numberOfObjects.on && (overwriteThis || parentOverwrite)) 
                {
                    
                    float iterator = elementVariation.phaseSeed != 0 ? elementVariation.iterator / elementVariation.phaseSeed
                        : elementVariation.iterator;
                    float decider = elementVariation.rand(elementVariation.phaseSeed, iterator, elementVariation.numberOfObjects);
                    //print($"decider = {decider} iterator = {elementVariation.iterator}");
                    if (elementVariation.overwriteProperties.numberOfObjects.type == NumberOfObjectsType.Random)
                    {
                        if ((int)((decider * 100) % 2) == 0)
                        {
                            shouldISpawn = false;
                        }
                    }
                    else if (elementVariation.overwriteProperties.numberOfObjects.type == NumberOfObjectsType.Limiting)
                    {
                        if (i == 0) elementVariation.overwriteProperties.numberOfObjects.limitingDecider = decider;
                        float limitingDecider = elementVariation.overwriteProperties.numberOfObjects.limitingDecider;
                        
                        if (i >= Math.Ceiling(limitingDecider * elementVariation.numberOfObjects)) {
                            shouldISpawn = false;
                        }
                    }
                }
                // Hold on a bit: if intended object has AnimationRandomizer, listen for the shortest duration 
                //if (g.GetComponent<AnimationRandomizer>() != null)
                //{
                //    if (i == 0 || g.GetComponent<AnimationRandomizer>().animDuration < minAnimDuration)
                //    {
                //        minAnimDuration = g.GetComponent<AnimationRandomizer>().animDuration;
                //        minAnim = g.GetComponent<AnimationRandomizer>();
                //        minAnimDurationIndex = i;
                //    }
                //}


                if (shouldISpawn && elementVariation.inReferenceFrame) 
                {
                    GameObject parent = new GameObject();
                    g = Instantiate(parent, transform);
                    Instantiate(element, g.transform);
                    DestroyImmediate(parent);
                }
                else if (shouldISpawn)
                {
                    g = Instantiate(element, transform);
                }
                else
                {
                    continue;
                }
                //set name
                g.name = gameObject.name + "_" + i;
                //set depth illusion for blue shift objects
                if (gameObject.GetComponent<DepthIllusion>() != null && g.GetComponent<BlueShift>() != null)
                { 
                    g.GetComponent<BlueShift>().depthController = gameObject;
                }
                //set depth illusion player for children with depth illusion (like VariableObjects or other ObjectSpawnSystems)
                if (gameObject.GetComponent<DepthIllusion>() != null && g.GetComponent<DepthIllusion>() != null) 
                {
                    g.GetComponent<DepthIllusion>().player = gameObject.GetComponent<DepthIllusion>().player;
                    g.GetComponent<DepthIllusion>().popBackController = gameObject.GetComponent<DepthIllusion>().popBackController;
                }
                //set blue shift for reference frame objects
                if (elementVariation.inReferenceFrame && gameObject.GetComponent<DepthIllusion>() != null)
                {
                    g.transform.GetChild(0).GetComponent<BlueShift>().depthController = gameObject;
                }
                //get scale
                Vector3 scale = elementVariation.scale;
                //potentially randomize scale
                if (elementVariation.overwriteProperties.scale && (overwriteThis || parentOverwrite))
                {
                    scale = multiplyVectors(elementVariation.scale, elementVariation.RandomVector3(elementVariation.phaseSeed, elementVariation.iterator, elementVariation.numberOfObjects));
                }
                //set scale
                if (elementVariation.inReferenceFrame) 
                {
                    g.transform.GetChild(0).localScale = scale;
                }
                else 
                {
                    g.transform.localScale = scale;
                }
                //get position
                Vector3 position = elementVariation.position;
                //potentially randomize position
                if (elementVariation.overwriteProperties.position && (overwriteThis || parentOverwrite))
                {
                    position = multiplyVectors(position, elementVariation.RandomVector3(elementVariation.phaseSeed, elementVariation.iterator, elementVariation.numberOfObjects));
                }
                //potentially randomize spawnSpacing
                Vector3 spawnSpacing = elementVariation.spawnSpacing;
                if (elementVariation.overwriteProperties.spawnSpacing && (overwriteThis || parentOverwrite))
                {
                    spawnSpacing = multiplyVectors(spawnSpacing, elementVariation.RandomVector3(elementVariation.phaseSeed, elementVariation.iterator, elementVariation.numberOfObjects));
                }
                //set position
                g.transform.position = transform.TransformPoint(spawnSpacing * i) + position;
                //set pop back position
                if (player != null && i == elementVariation.numberOfObjects - elementVariation.popBackAtLast) 
                {
                    elementVariation.popBackAtLastPosition = g.transform.position;
                    elementVariation.popBackTransform = g.transform;
                }
                //get rotation
                Vector3 rotation = transform.TransformVector(elementVariation.rotation);
                //potentially randomize rotation
                if (elementVariation.overwriteProperties.rotation && (overwriteThis || parentOverwrite))
                {
                    rotation += multiplyVectors(rotation, elementVariation.RandomVector3(elementVariation.phaseSeed, elementVariation.iterator, elementVariation.numberOfObjects));
                }
                //set rotation
                g.transform.eulerAngles = rotation;
                //calculate potentially random layer position
                int layerPosition = elementVariation.layerPosition;
                if (elementVariation.overwriteProperties.layerPosition && (overwriteThis || parentOverwrite)) 
                {
                    layerPosition = (int)(elementVariation.layerPosition * elementVariation.rand(elementVariation.phaseSeed, elementVariation.iterator, elementVariation.numberOfObjects));
                }
                //Layer position is for SpriteRenderers, so we have to check if g has a sprite renderer.
                if (g.GetComponent<SpriteRenderer>()) {
                    g.GetComponent<SpriteRenderer>().sortingOrder = layerPosition;
                }
                //calculate potentially random color
                Vector4 color = elementVariation.RandomVector4(elementVariation.phaseSeed, elementVariation.iterator, elementVariation.numberOfObjects);
                //reset blue shift to potential randomized color
                
                //potentially set random color
                if (elementVariation.overwriteProperties.color && (overwriteThis || parentOverwrite))
                {
                    if (elementVariation.overwriteProperties.randomizeChild && elementVariation.inReferenceFrame) 
                    {
                        g.transform.GetChild(0).GetChild(elementVariation.overwriteProperties.childIndex).gameObject.GetComponent<SpriteRenderer>().color = color;
                    }
                    else if (elementVariation.overwriteProperties.randomizeChild && (!elementVariation.inReferenceFrame))
                    {
                        g.transform.GetChild(elementVariation.overwriteProperties.childIndex).gameObject.GetComponent<SpriteRenderer>().color = color;
                    }
                    else
                    {
                        g.GetComponent<SpriteRenderer>().color = color;
                    }
                }
                
                
            }
            
            //if (elementVariation.animationRandomizerRequest) 
            //{
            //    if (minAnim.seed != elementVariation.phaseSeed) 
            //    {
            //        elementVariation.phaseSeed = minAnim.seed;
            //        Debug.Log($"phaseSeed = {elementVariation.phaseSeed}");
            //    }  
            //}
            

            ElementVariation oldElementVariation = elementVariation;
            yield return new WaitUntil(() => popBack || !elementVariation.Equals(oldElementVariation) || numAnimResetRequests == elementVariation.numberOfObjects);
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
        if (elementVariation.popBackTransform != null)
        {
            if (player != null && elementVariation.popBackTransform.position.x - player.transform.position.x
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
        //if this system does not issue popBack, check for popBack.
        bool externalPopBackSetup = player == null && popBackController != null;
        if (externalPopBackSetup) 
        {
            if (popBackController.GetComponent<ObjectSpawnSystem>().popBack) 
            {
                popBack = true;
            }
            
        }
    }
}
