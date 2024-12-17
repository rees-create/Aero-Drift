using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueShift : MonoBehaviour
{
    public GameObject depthController;
    // Start is called before the first frame update
    void Start()
    {
        DepthIllusion illusion = depthController.GetComponent<DepthIllusion>();
        Color color = gameObject.GetComponent<SpriteRenderer>().material.color;
        gameObject.GetComponent<SpriteRenderer>().material.color = illusion.ShiftBlue(color);
    }

 }
