using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueShift : MonoBehaviour
{
    public GameObject depthController;
    public Vector4 ShiftBlue(Vector4 color)
    {
        DepthIllusion illusion = depthController.GetComponent<DepthIllusion>();
        //print(illusion.absoluteBlue);
        Vector4 newColor = Vector4.Lerp(color, illusion.absoluteBlue, illusion.parallaxFraction);
        return newColor;
    }

    void Start()
    {
        DepthIllusion illusion = depthController.GetComponent<DepthIllusion>();
        Color color = gameObject.GetComponent<SpriteRenderer>().material.color;
        gameObject.GetComponent<SpriteRenderer>().material.color = ShiftBlue(color);
    }

}
