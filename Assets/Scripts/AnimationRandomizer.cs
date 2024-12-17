using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class AnimationRandomizer : MonoBehaviour
{
    public Animator animator;
    public float multiplier;
    public string propertyName;
    //ObjectSpawnSystem randomizer;
    // Start is called before the first frame update
    IEnumerator SpawnLoop(GameObject spawner) 
    {
        
        while (true) 
        {
            ObjectSpawnSystem spawnSystem = spawner.GetComponent<ObjectSpawnSystem>();
            float seed = spawnSystem.elementVariation.phaseSeed;
            float numberOfObjects = spawnSystem.elementVariation.numberOfObjects;
            float index = GetIndex(gameObject);
            float rand = spawnSystem.elementVariation.rand(seed, index, numberOfObjects);
            animator.SetFloat(propertyName, multiplier * rand);
            Debug.Log($"{index} speed = {multiplier * rand}");
            yield return new WaitUntil(() => spawnSystem.popBack || );
        }
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
        
        StartCoroutine(SpawnLoop(GetObjectSpawnSystem(gameObject)));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
