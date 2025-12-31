using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LiftSortingOrder : MonoBehaviour
{
    
    [SerializeField] int desiredSortingOrder;
    [SerializeField] bool setSortingOrder;
    void OnValidate()
    {
        if (setSortingOrder)
        {
            SwitchHierarchyLayer(gameObject, desiredSortingOrder);
            setSortingOrder = false;
        }
    }

    void SwitchHierarchyLayer(GameObject g, int layer, int upperSortingOrder = -1)
    {
        int layerDifference = 0;
        if (g.GetComponent<SpriteRenderer>())
        {
            layerDifference = layer - g.GetComponent<SpriteRenderer>().sortingOrder;
            upperSortingOrder = g.GetComponent<SpriteRenderer>().sortingOrder;
            g.GetComponent<SpriteRenderer>().sortingOrder = layer;
            //print($"{g.name} new layer = {layer}");
        }
        for (int index = 0; index < g.transform.childCount; index++)
        {
            GameObject child = g.transform.GetChild(index).gameObject;
            int layerVariation = 0;
            if (child.GetComponent<SpriteRenderer>() && upperSortingOrder != -1) //check for most recent g sprite renderer in tree 
            {
                layerVariation = upperSortingOrder - child.GetComponent<SpriteRenderer>().sortingOrder;
                //print($"layerVariation for child {child} = {layerVariation}, upper sorting order = {upperSortingOrder}, child order {child.GetComponent<SpriteRenderer>().sortingOrder}");
            }
            //recursively search children
            SwitchHierarchyLayer(child, layer - layerVariation, upperSortingOrder);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        OnValidate();
    }
}
