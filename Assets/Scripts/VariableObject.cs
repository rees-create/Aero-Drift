using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using static UnityEditor.Experimental.GraphView.GraphView;

[ExecuteInEditMode]
public class VariableObject : MonoBehaviour
{
    [SerializeField] List<GameObject> objects;    
    bool unparentAndSelfDestruct;
    [SerializeField] float decision;
    public int sortingOrder;
    // Start is called before the first frame update

    //public void SetSortingOrder(GameObject g, int desiredSortingOrder, int upperSortingOrder = -1)
    //{
    //    int layerDifference = 0;  
    //    if (g.GetComponent<SpriteRenderer>())
    //    {
    //        g.transform.GetComponent<SpriteRenderer>().sortingOrder = desiredSortingOrder;
    //        layerDifference = desiredSortingOrder - g.GetComponent<SpriteRenderer>().sortingOrder;
    //        upperSortingOrder = g.GetComponent<SpriteRenderer>().sortingOrder;
    //        //Debug.Log(g.name + " sorting order: " + g.GetComponent<SpriteRenderer>().sortingOrder);
    //    }
    //    if (g.transform.childCount > 0)
    //    {
    //        //Debug.Log("Descending hierarchy: " + g.name + " desired sorting order: " + desiredSortingOrder);
    //        for (int i = 0; i < g.transform.childCount; i++)
    //        {
    //            //SetSortingOrder(g.transform.GetChild(i).gameObject, desiredSortingOrder + i);
    //            int layerVariation = 0;
    //            if (g.transform.GetChild(i).GetComponent<SpriteRenderer>() && upperSortingOrder != -1) //check for most recent g sprite renderer in tree 
    //            {
    //                layerVariation = upperSortingOrder - g.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder;
    //                //print($"layerVariation for child {child} = {layerVariation}, upper sorting order = {upperSortingOrder}, child order {child.GetComponent<SpriteRenderer>().sortingOrder}");
    //            }
    //            //recursively search children
    //            SetSortingOrder(g.transform.GetChild(i).gameObject, desiredSortingOrder - layerVariation, upperSortingOrder);
    //        }
    //    }
    //    //if (g.GetComponent<VariableObject>())
    //    //{
    //    //    g.GetComponent<VariableObject>().sortingOrder = desiredSortingOrder;
    //    //}
    //}

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

        void ApplyBlueShift(GameObject g, DepthIllusion illusion)
    {
        for (int i = 0; i < g.transform.childCount; i++)
        {
            if (g.transform.GetChild(i).GetComponent<BlueShift>() != null)
            {
                g.transform.GetChild(i).GetComponent<BlueShift>().depthController = gameObject;
            }
            else //recursively search children till you find blue shift
            {
                ApplyBlueShift(g.transform.GetChild(i).gameObject, illusion);
            }
        }
    }
    T FindComponentInParent<T>(GameObject g) 
    {
        if (!g.transform.parent || g.GetComponentInParent<T>() != null)
        {
            return g.GetComponentInParent<T>();
        }
        else 
        {
            return FindComponentInParent<T>(g.transform.parent.gameObject);
        }
    }
    int HighSiblingIndex<T>(GameObject g)
    {
        if (g.transform.parent.GetComponent<T>() == null)
        {
            return HighSiblingIndex<T>(g.transform.parent.gameObject);
        }
        return g.transform.GetSiblingIndex();
    }
    // Update is called once per frame
    IEnumerator PickObject()
    {
        //choose object to instantiate
        ObjectSpawnSystem objectSpawnSystem = FindComponentInParent<ObjectSpawnSystem>(gameObject);//gameObject.GetComponentInParent<ObjectSpawnSystem>();
        //print(objectSpawnSystem.elementVariation.phaseSeed);
        if (objectSpawnSystem != null)
        {
            decision = objectSpawnSystem.elementVariation.rand(objectSpawnSystem.elementVariation.phaseSeed, HighSiblingIndex<ObjectSpawnSystem>(gameObject), 0);
        }
        else
        {
            decision = Random.Range(0f, 1f);
        }
        yield return new WaitUntil(() => objects != null);
        int objectIndex = (int)(decision * objects.Count);
        //Wait until objects array is populated
        int nullCount = objects.Count;
        for (int i = 0; i < objects.Count; i++) 
        {
            if (objects[i] != null) nullCount--;
        }
        yield return new WaitUntil(() => objects.Count > 0 && nullCount == 0);
        SetSortingOrder(objects[objectIndex], sortingOrder);
        //Instantiate object
        Instantiate(objects[objectIndex], transform);
        //Set BlueShift and DepthIllusion properties if applicable.
        
        if (GetComponent<DepthIllusion>() != null && transform.parent.gameObject.GetComponent<DepthIllusion>() != null)
        {
            
            DepthIllusion illusion = gameObject.GetComponent<DepthIllusion>();
            ApplyBlueShift(gameObject, illusion);
        }

        float oldDecision = decision;
        yield return new WaitUntil(()=> oldDecision != decision);
    }

    [ExecuteInEditMode]
    void Start()
    {
        StartCoroutine(PickObject());
    }
    void FixedUpdate() 
    { }
}
