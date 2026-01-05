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
            SetSortingOrder(gameObject, desiredSortingOrder);
            setSortingOrder = false;
        }
    }

    public void SetSortingOrder(GameObject g, int desiredSortingOrder, int upperSortingOrder = -1)
    {
        //TODO: relative sorting order preservation.
        int layerDifference = 0;
        if (g.GetComponent<SpriteRenderer>() || g.GetComponent<VariableObject>())
        {

            if (g.GetComponent<SpriteRenderer>())
            {
                layerDifference = desiredSortingOrder - g.GetComponent<SpriteRenderer>().sortingOrder;
                upperSortingOrder = g.GetComponent<SpriteRenderer>().sortingOrder;
                g.transform.GetComponent<SpriteRenderer>().sortingOrder = desiredSortingOrder;
            }
            else
            {
                layerDifference = desiredSortingOrder - g.GetComponent<VariableObject>().sortingOrder;
                upperSortingOrder = g.GetComponent<VariableObject>().sortingOrder;
                g.transform.GetComponent<VariableObject>().sortingOrder = desiredSortingOrder;
            }
            //Debug.Log(g.name + " sorting order: " + g.GetComponent<SpriteRenderer>().sortingOrder);
        }
        //Debug.Log("Descending hierarchy: " + g.name + " desired sorting order: " + desiredSortingOrder);
        for (int i = 0; i < g.transform.childCount; i++)
        {
            int layerVariation = 0;
            if (g.transform.GetChild(i).GetComponent<SpriteRenderer>() && upperSortingOrder != -1) //check for most recent g sprite renderer in tree 
            {
                layerVariation = upperSortingOrder - g.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder;
                //print($"layerVariation for child {child} = {layerVariation}, upper sorting order = {upperSortingOrder}, child order {child.GetComponent<SpriteRenderer>().sortingOrder}");
            }
            if (g.transform.GetChild(i).GetComponent<VariableObject>() && upperSortingOrder != -1) //check for most recent g sprite renderer in tree 
            {
                layerVariation = upperSortingOrder - g.transform.GetChild(i).GetComponent<VariableObject>().sortingOrder;
            }
            if (g.transform.childCount > 0)
            {
                SetSortingOrder(g.transform.GetChild(i).gameObject, desiredSortingOrder - layerVariation, upperSortingOrder);
            }
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
