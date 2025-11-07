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
    [Header("IX Control")]
    public bool active;
    float destroyTimeLimit;
    [Header("Animations")]
    public AnimationClip planeCrumple;
    public AnimationClip handCrumple;
    public AnimationClip stomp;
    public GameObject plane;
    public GameObject hand;
    public GameObject foot;

    public float catchRadius;
    public float stompRadius;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    IEnumerator CatchAndDestroy(CrumpleType crumpleType) 
    {
        GetComponent<CatchPlane>().catchRadius += 3.5f; //hahaha
        GetComponent<CatchPlane>().active = true;
        yield return new WaitUntil(()=>GetComponent<CatchPlane>().active == false); // until plane is caught
        StartCoroutine(Crumple(crumpleType));
        plane.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic; //let it fall;
    }

    enum CrumpleType { Hand, Foot }
    IEnumerator Crumple(CrumpleType type) 
    {
        float t = 0;
        while (t < 1) 
        {
            planeCrumple.SampleAnimation(plane, t);
            if (type == CrumpleType.Hand)
            {
                handCrumple.SampleAnimation(hand, t);
            }
            else if (type == CrumpleType.Foot)
            { 
                stomp.SampleAnimation(foot, t);
            }
            t++;
            yield return new WaitForEndOfFrame();
        }
        StopCoroutine(Crumple(type));
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
                StartCoroutine(CatchAndDestroy(CrumpleType.Hand));
            }
        }
        if (withinStompRadius)
        {
            //stomp on plane to destroy
            StartCoroutine(Crumple(CrumpleType.Foot));
        }
    }
}
