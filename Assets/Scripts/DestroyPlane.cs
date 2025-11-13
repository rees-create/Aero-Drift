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
        StartCoroutine(ActionLoop());
    }

    IEnumerator CatchAndDestroy(CrumpleType crumpleType) 
    {
        print("catch and destroy started");
        //GetComponent<CatchPlane>().catchRadius += 3.5f; //hahaha
        GetComponent<CatchPlane>().active = true;
        yield return new WaitUntil(()=>GetComponent<CatchPlane>().active == false); // until plane is caught
        print("catch plane deactivated");
        StartCoroutine(Crumple(crumpleType));
        //let it fall;
        plane.GetComponent<PolygonCollider2D>().enabled = true;
        plane.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        plane.GetComponent<Rigidbody2D>().gravityScale = 1;
        StopCoroutine(CatchAndDestroy(crumpleType));
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
            t += Time.deltaTime / 0.3f; //crumple in 0.3 seconds
            yield return new WaitForEndOfFrame();
        }
        print("plane crumpled");
        StopCoroutine(Crumple(type));
    }

    // Update is called once per frame
    IEnumerator ActionLoop()
    {
        while (true)
        { //game loop
            while (active)
            {
                
                bool withinCatchRadius = Vector3.Distance(plane.transform.position, transform.position) < catchRadius;
                bool withinStompRadius = Vector3.Distance(foot.transform.position, plane.transform.position) < stompRadius;

                //yield return new WaitUntil(() => withinCatchRadius || withinStompRadius);
                
                if (withinCatchRadius)
                {
                    print("within catch range");
                    //catch and destroy plane
                    if (GetComponent<CatchPlane>())
                    {
                        StartCoroutine(CatchAndDestroy(CrumpleType.Hand));
                        active = false;
                    }
                }
                if (withinStompRadius)
                {
                    //stomp on plane to destroy
                    print("within stomp range");
                    StartCoroutine(Crumple(CrumpleType.Foot));
                    active = false;
                }
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitUntil(() => active);
        }
    }


}
