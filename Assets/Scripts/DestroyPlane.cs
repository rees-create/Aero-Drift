using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPlane : MonoBehaviour
{
    // The DestroyPlane action is
    // folding "crumple bones" on the plane.
    // If the plane is neither:
    // 1. in hands 2. at feet's range
    // activate CatchPlane in Destroy mode.

    //If none are active within a time limit, deactivate DestroyPlane.
    bool active;
    float destroyTimeLimit;
    AnimationClip planeCrumple;
    AnimationClip handCrumple;
    GameObject plane;
    GameObject foot;

    float catchRadius;
    float stompRadius;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    IEnumerator CatchAndWait() 
    {
        GetComponent<CatchPlane>().catchRadius += 3.5f; //hahaha
        GetComponent<CatchPlane>().active = true;
        yield return new WaitUntil(()=>GetComponent<CatchPlane>().active == false); //until plane is caught
    }
    IEnumerator PlaneCrumple() 
    {
        float t = 0;
        while (t < 1) 
        {
            planeCrumple.SampleAnimation(plane, t);
            t++;
            yield return new WaitForEndOfFrame();
        }
        StopCoroutine(PlaneCrumple());
    }
    // Update is called once per frame
    void Update()
    {
        bool withinCatchRadius = Vector3.Distance(plane.transform.position, gameObject.transform.position) < catchRadius;
        bool withinStompRadius = Vector3.Distance(foot.transform.position, gameObject.transform.position) < stompRadius;
        if (withinCatchRadius) {
            //catch and destroy plane
            if (GetComponent<CatchPlane>()) 
            {
                StartCoroutine(CatchAndWait());


            }
        }
        if (withinStompRadius)
        {
            //stomp on plane to destroy
        }
    }
}
