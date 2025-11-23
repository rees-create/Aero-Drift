using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSwitchLayer : MonoBehaviour
{
    //NPC Switch Layer Action Script
    //Sequence:
    // 1. Walk toward layer switch point
    // 2. Perform the object's layer switch action
    // 3. Switch layers. Floor Control has the layer sorting orders listed.
    // 4. Perform any after-switch actions (e.g. shutting door)
    // Start is called before the first frame update
    bool active;
    public int currentLayer;
    public FloorControl floorController;
    public LayerColliderMap NPCLayerColliderMap;
    public AnimationClip layerSwitchAnimationBefore;
    public AnimationClip layerSwitchAnimationAfter;
    

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
                gameObject.GetComponent<Pusher>().active = true;
                yield return new WaitUntil(() => !gameObject.GetComponent<Pusher>().active); //wait for pusher to finish
                //2
                if (layerSwitchAnimationBefore != null && layerSwitchAnimationBefore.length > 0)
                {
                    float localTime = 0;
                    while (localTime < layerSwitchAnimationBefore.length)
                    {
                        localTime += Time.fixedDeltaTime;
                        layerSwitchAnimationBefore.SampleAnimation(gameObject, localTime);
                        yield return new WaitForFixedUpdate();
                    }
                }
                //3
                SwitchLayer(targetColliderInfo.Value);
                //4
                if (layerSwitchAnimationAfter != null && layerSwitchAnimationAfter.length > 0)
                {
                    float localTime = 0;
                    while (localTime < layerSwitchAnimationAfter.length)
                    {
                        localTime += Time.fixedDeltaTime;
                        layerSwitchAnimationAfter.SampleAnimation(gameObject, localTime);
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
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = floorController.layers[currentLayer].sortingOrder;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
