using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationRandomizer : MonoBehaviour
{
    public Animator animator;
    public float animDuration;
    public float speedMultiplier;
    public float minSpeed;
    public string propertyName;
    public string animationClipName;
    public bool bounceInfluence;
    [NonSerialized] public float seed;
    GameObject spawner;
    //ObjectSpawnSystem randomizer;
    // Start is called before the first frame update
    IEnumerator SpawnLoop() 
    {
        float seedJump = spawner.GetComponent<ObjectSpawnSystem>().elementVariation.seedJump;
        seed = spawner.GetComponent<ObjectSpawnSystem>().elementVariation.phaseSeed;
        ObjectSpawnSystem spawnSystem = spawner.GetComponent<ObjectSpawnSystem>();
      
        while (true) 
        {
             
            float numberOfObjects = spawnSystem.elementVariation.numberOfObjects;
            float index = GetIndex(gameObject);
            float rand = spawnSystem.elementVariation.rand(seed, index, numberOfObjects);
            float speed = speedMultiplier * rand;
            animator.SetFloat(propertyName, speed);
            // on cycle refresh we have the new seed yay :))
            if (bounceInfluence)
            {
                spawner.GetComponent<ObjectSpawnSystem>().elementVariation.InitiateSeedJump(this);
            }
            animDuration = GetAnimClipDuration(animationClipName) / (speed + minSpeed);
            Debug.Log($"{index} speed = {speed} animator duration = {animDuration} seed = {spawner.GetComponent<ObjectSpawnSystem>().elementVariation.phaseSeed}");
            yield return new WaitForSeconds(animDuration);
            //yield return new WaitUntil(() => spawnSystem.popBack);
            seed += seedJump;
        }
    }

    float GetAnimClipDuration(string name) 
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips) 
        {
            if (clip.name == name) 
            {
                return clip.length;
            }
        }
        return float.PositiveInfinity; //yea make it basically wait forever
    }
    
    GameObject GetObjectSpawnSystem(GameObject start) 
    {
        Transform tr = start.transform.parent;
        if (tr.GetComponent<ObjectSpawnSystem>() == null) 
        {
            return GetObjectSpawnSystem(tr.gameObject);
        }
        else 
        {
            return tr.gameObject;
        }
    }
    int GetNumFromString(string str) 
    {
        int delimiter_pos = str.IndexOf('_');
        if (delimiter_pos == -1) 
        {
            return -1;
        }
        int index = 0;
        int pow = 0;
        for (int i = str.Length - 1; i > delimiter_pos; i--)
        {
            if (str[i] - '0' > 9 || str[i] - '0' < 0) return -1;
            index += (str[i] - '0') * (int)Mathf.Pow(10, pow);
            pow++;
        }
        return index;
    }
    int GetIndex(GameObject start)
    {
        Transform tr = start.transform.parent;
        //string digits = "0123456789";
        
        if (GetNumFromString(tr.gameObject.name) == -1)
        {
            return GetIndex(tr.gameObject);
        }
        else
        {
            return GetNumFromString(tr.gameObject.name);
        }
    }
    void Start()
    {
        spawner = GetObjectSpawnSystem(gameObject);
        StartCoroutine(SpawnLoop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
