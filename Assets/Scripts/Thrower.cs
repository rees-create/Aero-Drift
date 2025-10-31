using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class Thrower : MonoBehaviour
{
    [Header("Activation")]
    public bool active;
    [Header("Game Objects")]
    public GameObject plane;
    public GameObject dash;
    public GameObject bait;
    public GameObject player; //for control over PoseLerp
    [Header("Animation Properties")]
    public Animator animator;
    [SerializeField] float minFollowWaitTime;
    public string animationName;
    public string triggerName;
    [Header("Throw Dash Settings")]
    public Vector3 dashScale;
    public float dashQuotient;
    [System.NonSerialized] public GameObject dashes;
    [Header("Throw Intensity")]
    public float maxThrowIntensity;

    Vector3 pointerPosition;
    Vector3 pointerWorldPosition;
    
    bool thrown = false;
    bool follow = true;
    Vector2 throwVector;
    Vector3 initPlaneRotation;

    float[] throwNormalizedTimes = new float[2];
    int diffSwitch = 0;
    bool throwNormalizedTimeReady = false;
    float throwNormalizedTime;
    float duration;
    
    int nDashes = 0;
    Vector3 oldBaitPosition;
    //PoseLerp throwFlex;
    Vector3[] baitPosDiffs = new Vector3[2];
    
    void MakeDashes(float throwLineLength, Vector3 mousePosition) 
    {
        //This is intended to be used in Update, so make sure to do if checks before running things.
        int numberOfDashes = Mathf.FloorToInt(throwLineLength / dashQuotient);
        int diff = numberOfDashes - nDashes;
        Vector3 plane2Mouse = mousePosition - plane.transform.position;
        Vector3 currentRotation = new Vector3(0, 0, Mathf.Atan2(plane2Mouse.y, plane2Mouse.x) * Mathf.Rad2Deg);
        Vector3 planeOrientation = currentRotation + initPlaneRotation; 
        plane.transform.eulerAngles = planeOrientation;
        if (diff < 0 && nDashes < 75)
        { //less dashes
            for (int i = 0; i < Mathf.Abs(diff); i++) 
            {
                Destroy(dashes.transform.GetChild(i).gameObject);
                nDashes--;
                diff++;
            }
        }
        else if (diff > 0 && nDashes < 75) 
        { //more dashes
            int initChildCount = dashes.transform.childCount;
            for (int i = 0; i < diff; i++) 
            {
                float lerpFraction = numberOfDashes != 0 ? (float) (i + initChildCount) / (float) numberOfDashes : 1f;
                GameObject _dash = Instantiate(dash, dashes.transform);
                _dash.transform.position = Vector2.Lerp(plane.transform.position, mousePosition, lerpFraction);
                _dash.transform.eulerAngles = currentRotation;
                dashes.transform.localScale = dashScale;
                nDashes++;
                diff--;
            }
            
        }
        if (nDashes > 0) 
        {
            for (int i = 0; i < dashes.transform.childCount; i++)
            {
                float lerpFraction = numberOfDashes != 0 ? (float) i / (float) numberOfDashes : 1f;
                dashes.transform.GetChild(i).position = Vector2.Lerp(plane.transform.position, mousePosition, lerpFraction);
                dashes.transform.GetChild(i).eulerAngles = currentRotation;
            }
        }

    }
    void Follow()
    {
        Vector3 newBaitPosition = transform.TransformPoint(bait.transform.position);
        if (oldBaitPosition != default)
        {
            plane.transform.position += newBaitPosition - oldBaitPosition;
        }
        oldBaitPosition = newBaitPosition;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        //disable physics, make plane face dashes
        dashes = new GameObject ("Throw Dashes");
        dashes.transform.parent = transform;
        plane.GetComponent<FlightControl>().enabled = false;
        plane.GetComponent<Rigidbody2D>().gravityScale = 0;
        plane.GetComponent<PolygonCollider2D>().enabled = false;
        initPlaneRotation = plane.transform.eulerAngles;
        //throwFlex = player.GetComponent<PoseLerp>();
        //get clip duration
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip cl = clips[i];
            if (cl.name == animationName)
            {
                duration = cl.length;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        diffSwitch = (diffSwitch + 1) % 2;
        throwNormalizedTimes[diffSwitch] = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        throwNormalizedTime = throwNormalizedTimes[diffSwitch];
        if (!thrown)
        {
            animator.enabled = false;
            pointerPosition = Input.mousePosition;
            pointerWorldPosition = Camera.main.ScreenToWorldPoint(pointerPosition);
            throwVector = plane.transform.position - pointerWorldPosition;
            float throwIntensity = throwVector.magnitude;
            MakeDashes(throwIntensity, pointerWorldPosition);
            //animate PoseLerp using throw intensity
            player.GetComponent<PoseLerp>().lerpValue = throwIntensity / maxThrowIntensity;
        }
        else
        {
            animator.enabled = true;
            //baitPosDiffs[diffSwitch] = bait.transform.position;
            //print($"bait pos diff: {baitPosDiffs[1] - baitPosDiffs[0]}");
            if (Mathf.Abs(throwNormalizedTimes[0] - throwNormalizedTimes[1]) * duration > 2.5 * Time.deltaTime)
            {
                throwNormalizedTimeReady = true;
            }
        }
        
        if (follow && !throwNormalizedTimeReady) 
        {
            Follow();
        }
        
    }
    private void LateUpdate()
    {
        if (follow)
        {
            Follow();
        }
    }


    IEnumerator OnMouseDown()
    {
        
        thrown = true;
        //play animation if there is a bait (animated object to follow)
        if (bait != null && animator != null && animationName != null && triggerName != null)
        { 
            //play animation, toggle follow
            animator.SetTrigger(triggerName);
            follow = true;
            yield return new WaitUntil(()=> throwNormalizedTimeReady && throwNormalizedTime >= 1);
            follow = false;
            
        }
        //launch plane
        plane.GetComponent<Rigidbody2D>().gravityScale = 1;
        plane.GetComponent<FlightControl>().enabled = true;
        plane.GetComponent<PolygonCollider2D>().enabled = true;
        
        if (throwVector.magnitude < maxThrowIntensity)
        {
            plane.GetComponent<FlightControl>().initialThrowImpulse = -throwVector;
        }
        else 
        {
            float throwIntensity = throwVector.magnitude;
            Vector2 throwDirection = throwVector / throwIntensity;
            plane.GetComponent<FlightControl>().initialThrowImpulse = -maxThrowIntensity * throwDirection;
        }

        Destroy(dashes);
    }
}
