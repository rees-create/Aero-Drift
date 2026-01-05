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
    //public int sortingOrder;
    public FloorControl floorController;
    public LayerColliderMap NPCLayerColliderMap;
    public AnimationClip beforeSwitchAnim;
    public Vector2 beforeSwitchTranslation;
    public AnimationClip afterSwitchAnim;
    public Vector2 afterSwitchTranslation;
    

    IEnumerator SwitchLayerSequence() 
    {
        while (true) 
        {
            // Detect and handle action scripts that can cause conflicts
            //1. Thrower:
            
            yield return new WaitWhile(() => gameObject.GetComponent<NPCThrower>().active);
            
            yield return new WaitUntil(() => active);
            
            if (gameObject.GetComponent<Pusher>())
            {
                //1
                KeyValuePair<Vector2, int> targetColliderInfo = FindClosestLayerCollider(NPCLayerColliderMap);
                gameObject.GetComponent<Pusher>().target = targetColliderInfo.Key;
                print("Target: escalator");
                gameObject.GetComponent<Pusher>().active = true;
                yield return new WaitUntil(() => !gameObject.GetComponent<Pusher>().active); //wait for pusher to finish
                print("to 2");
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
                //if beforeSwitch has translation use pusher
                if (beforeSwitchTranslation != default) { 
                    gameObject.GetComponent<Pusher>().target = (Vector2) transform.position + beforeSwitchTranslation;
                    gameObject.GetComponent<Pusher>().active = true;
                    print("translating before switch..");
                    yield return new WaitForSeconds(2);
                    yield return new WaitUntil(() => !gameObject.GetComponent<Pusher>().active); //wait for pusher to finish
                    print("switch layer");
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
                if (afterSwitchTranslation != default)
                {
                    gameObject.GetComponent<Pusher>().target = (Vector2)transform.position + afterSwitchTranslation;
                    gameObject.GetComponent<Pusher>().active = true;
                    yield return new WaitUntil(() => !gameObject.GetComponent<Pusher>().active); //wait for pusher to finish
                }
            }
            active = false;
        }
    }

    void SwitchLayer(int direction)
    {
        //wait for clear target in: Current layer if moving up, next layer if moving down.
        int nextLayer = currentLayer + direction;
        bool nextLayerInBounds = nextLayer > 0 && nextLayer < floorController.layers.Count;
        
        currentLayer = (nextLayer >= 0 && nextLayer < floorController.layers.Count) ? nextLayer : currentLayer;
        //gameObject.GetComponent<SpriteRenderer>().sortingOrder = floorController.layers[currentLayer].sortingOrder; // instead, recurse through player hierarchy when applicable
        SwitchHierarchyLayer(gameObject, floorController.layers[currentLayer].sortingOrder);
        //Also change layer collider map.
        NPCLayerColliderMap = floorController.layers[currentLayer].colliderMap;
        print("switching layer");
    }
    void SwitchHierarchyLayer(GameObject g, int layer, int upperSortingOrder = -1) {
        int layerDifference = 0;
        if (g.GetComponent<SpriteRenderer>())
        {
            layerDifference = layer - g.GetComponent<SpriteRenderer>().sortingOrder;
            upperSortingOrder = g.GetComponent<SpriteRenderer>().sortingOrder;
            g.GetComponent<SpriteRenderer>().sortingOrder = layer;
            //print($"{g.name} new layer = {layer}");
        }
        for (int index = 0; index < g.transform.childCount; index++) {
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
                    closest = collider.position;
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
