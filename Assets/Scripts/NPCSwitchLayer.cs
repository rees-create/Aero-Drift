using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NOTE: This script scrapes values from other behaviors in the scene, including: FloorControl, LayerColliderMap
/// for functionality.
/// </summary>

public class NPCSwitchLayer : MonoBehaviour
{
    //NPC Switch Layer Action Script
    //Sequence:
    // 1. Walk toward layer switch point
    // 2. Perform the object's layer switch action
    // 3. Switch layers. Floor Control has the layer sorting orders listed.
    // 4. Perform any after-switch actions (e.g. shutting door)
    // Start is called before the first frame update
    public bool active;
    public int currentLayer;
    public FloorControl floorController;
    public LayerColliderMap NPCLayerColliderMap;
    public AnimationClip beforeSwitchAnim;
    public AnimationClip afterSwitchAnim;
    

    IEnumerator SwitchLayerSequence() 
    {
        while (true) 
        {
            yield return new WaitUntil(() => active);
            
            if (gameObject.GetComponent<Pusher>())
            {
                //1
                KeyValuePair<Vector2, int> targetColliderInfo = FindClosestLayerCollider(NPCLayerColliderMap);
                gameObject.GetComponent<Pusher>().target = targetColliderInfo.Key;
                //print("Target: escalator");
                gameObject.GetComponent<Pusher>().active = true;
                yield return new WaitUntil(() => !gameObject.GetComponent<Pusher>().active); //wait for pusher to finish
                //2
                if (beforeSwitchAnim != null && beforeSwitchAnim.length > 0)
                {
                    float localTime = 0;
                    while (localTime < beforeSwitchAnim.length)
                    {
                        localTime += Time.fixedDeltaTime;
                        beforeSwitchAnim.SampleAnimation(gameObject, localTime);
                        yield return new WaitForFixedUpdate();
                    }
                }
                //3
                SwitchLayer(targetColliderInfo.Value);
                //4
                if (afterSwitchAnim != null && afterSwitchAnim.length > 0)
                {
                    float localTime = 0;
                    while (localTime < afterSwitchAnim.length)
                    {
                        localTime += Time.fixedDeltaTime;
                        afterSwitchAnim.SampleAnimation(gameObject, localTime);
                        yield return new WaitForFixedUpdate();
                    }
                }
            }
        }
    }

    void SwitchLayer(int direction)
    {
        //wait for clear target in: Current layer if moving up, next layer if moving down.
        int nextLayer = currentLayer + direction;
        bool nextLayerInBounds = nextLayer > 0 && nextLayer < floorController.layers.Count;
        
        currentLayer = (nextLayer >= 0 && nextLayer < floorController.layers.Count) ? nextLayer : currentLayer;
        //gameObject.GetComponent<SpriteRenderer>().sortingOrder = floorController.layers[currentLayer].sortingOrder; // instead, recurse through player hierarchy when applicable
        SwitchHierarchyLayer(gameObject,currentLayer);
        //Also change layer collider map.
        NPCLayerColliderMap = floorController.layers[currentLayer].colliderMap;
    }
    void SwitchHierarchyLayer(GameObject g, int layer) {
        if (g.GetComponent<SpriteRenderer>())
        {
            g.GetComponent<SpriteRenderer>().sortingOrder = layer;
        }
        for (int index = 0; index < g.transform.childCount; index++) {
            GameObject child = g.transform.GetChild(index).gameObject;
            if (child.GetComponent<SpriteRenderer>())
            {
                //assign layer
                int layerDifference = layer - child.GetComponent<SpriteRenderer>().sortingOrder; //?? g.GetComponent<SpriteRenderer>().sortingOrder;
                layer += layerDifference;
                child.GetComponent<SpriteRenderer>().sortingOrder = layer;
                layer -= layerDifference;
            }
            if (child.transform.childCount > 0)
            {
                //recursively search child
                SwitchHierarchyLayer(child, layer);
            }
        }
    }    
    KeyValuePair<Vector2, int> FindClosestLayerCollider(LayerColliderMap layerColliderMap) 
    {
        Vector2 closest = layerColliderMap.colliders[0].position;
        int closestIndex = 0;
        int index = 0;
        foreach (var collider in layerColliderMap.colliders) {
            index++;
            if (collider.on)
            {
                if(Vector2.Distance(transform.position, collider.position) < Vector2.Distance(transform.position, closest))
                {
                    closestIndex = index;
                }
            }
        }
        int directionToLayer = layerColliderMap.colliders[closestIndex].directionToLayer;
        return new KeyValuePair<Vector2, int>(closest, directionToLayer);
    }

    void Start()
    {
        StartCoroutine(SwitchLayerSequence());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
