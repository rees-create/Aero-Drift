using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeSpeed : MonoBehaviour
{
    public Animator move;
    public float randFrequency;
    public float rand(float seed, float index)
    {
        float frequency = randFrequency;
        float divisor = seed != 0 ? seed : 1;
        float waveValue = Mathf.Abs(Mathf.Sin(Mathf.PI * frequency * (index / divisor)) +
            Mathf.Sin((Mathf.PI * frequency * index) + seed))
            % 1;
        //return ((seed + (seed * Mathf.Pow(2, index))) % numberOfObjects) / numberOfObjects;
        return waveValue;
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
