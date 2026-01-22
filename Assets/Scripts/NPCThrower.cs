using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class NPCThrower : MonoBehaviour
{
    int dashQuotient = 2;
    int nDashes = 0;
    Vector3 initPlaneRotation; //auto set on active.
    Vector3 pointerPosition;
    Vector3 throwVector;
    [System.NonSerialized] public GameObject dashes;
    [Header("Activation")]
    public bool active;
    
    bool thrown;
    [Header("Game Objects")]
    [SerializeField] GameObject plane;
    [SerializeField] GameObject bait;
    [SerializeField] GameObject dash;
    [Header("Throw Customization")]
    [SerializeField] Vector2 dashScale;
    [SerializeField] Vector2 followOffset;
    [Header("Animation Properties")]
    [SerializeField] AnimationClip throwAnimation;
    [SerializeField] bool usePoseLerp;
    [Range(0,1)]
    [SerializeField] float preLaunch;
    [Header("Throw Intensity")]
    [SerializeField] float maxThrowIntensity;
    [SerializeField] float throwIntensityScale;
    [Header("Plane Orientation")]
    [SerializeField] bool setInitPlaneRotation;
    [SerializeField] Vector3 defaultPlaneRotation;
    [Header("Audio")]

    int oldThrowerCount = 0;
    int newThrowerCount = 0;

    public void SetActive() {
        float baitToPlaneDistance = Vector2.Distance((Vector2)bait.transform.position, (Vector2) plane.transform.position);
        if (baitToPlaneDistance < followOffset.magnitude)
        {
            active = true;
            newThrowerCount++;
            
            //enabled = true;
            print($"{gameObject.name}: Thrower activated, newThrowerCount = {newThrowerCount}");
        }
        else {
            print($"Too far, bait to plane = {baitToPlaneDistance}, radius to stay within = {followOffset.magnitude}");
            print($"bait: {(Vector2) bait.transform.position} plane: {(Vector2) plane.transform.position}");
        }
    }
    //Utility functions
    void MakeDashes(float throwLineLength, Vector3 mousePosition)
    {
        //This is intended to be used in Update, so make sure to do if checks before running things.
        int numberOfDashes = Mathf.FloorToInt(throwLineLength / dashQuotient);
        int diff = numberOfDashes - nDashes;
        Vector3 plane2Mouse = mousePosition - plane.transform.position;
        Vector3 currentRotation = new Vector3(0, 0, Mathf.Atan2(plane2Mouse.y, plane2Mouse.x) * Mathf.Rad2Deg);
        Vector3 planeOrientation = currentRotation + initPlaneRotation;
        plane.transform.eulerAngles = planeOrientation;
        if (dashes != null)
        {
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
                    float lerpFraction = numberOfDashes != 0 ? (float)(i + initChildCount) / (float)numberOfDashes : 1f;
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
                    float lerpFraction = numberOfDashes != 0 ? (float)i / (float)numberOfDashes : 1f;
                    dashes.transform.GetChild(i).position = Vector2.Lerp(plane.transform.position, mousePosition, lerpFraction);
                    dashes.transform.GetChild(i).eulerAngles = currentRotation;
                }
            }
        }

    }

    void Follow(GameObject bait, Vector2 offset)
    {
        plane.transform.position = bait.transform.position + (Vector3) offset;
    }
    // Throw sequence functions
    IEnumerator InitThrow() 
    {
        if (active)
        {
            dashes = new GameObject("Throw Dashes");
            dashes.transform.parent = transform;
            plane.GetComponent<FlightControl>().AoA = 0;
            plane.GetComponent<FlightControl>().flapAngle = 0;
            plane.GetComponent<FlightControl>().enabled = false;
            plane.GetComponent<Rigidbody2D>().gravityScale = 0;
            plane.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            plane.GetComponent<PolygonCollider2D>().enabled = false;
            if (setInitPlaneRotation) 
            {
                plane.transform.eulerAngles = defaultPlaneRotation;
                //initPlaneRotation = defaultPlaneRotation;
            }
            initPlaneRotation = plane.transform.eulerAngles;
            if (usePoseLerp)
            {
                gameObject.GetComponent<PoseLerp>().poseSequenceManager.play = true;
            }
        }
        yield return new WaitUntil(() => thrown);
    }
    IEnumerator TauntThrow() 
    {
        while (!thrown)
        {
            //yield return new WaitWhile(() => thrown);

            if (active)
            {
                //track mouse
                pointerPosition = Input.mousePosition;
                Vector3 pointerWorldPosition = Camera.main.ScreenToWorldPoint(pointerPosition);
                //calculate throw intensity from mouse position
                throwVector = plane.transform.position - pointerWorldPosition;
                float throwIntensity = throwVector.magnitude;
                float weightedThrowIntensity = throwIntensity * throwIntensityScale;
                //sample throw animation to match intensity
                if (!usePoseLerp)
                {
                    throwAnimation.SampleAnimation(gameObject, 1 - Mathf.Min(weightedThrowIntensity / maxThrowIntensity, 1f));
                    //print($"normalized intensity = {1 - Mathf.Min(weightedThrowIntensity / maxThrowIntensity, 1f)}");
                }
                else
                {
                    gameObject.GetComponent<PoseLerp>().lerpValue = 1 - Mathf.Min(weightedThrowIntensity / maxThrowIntensity, 1f);
                }
                //throwAnimation.length - (Mathf.Min(weightedThrowIntensity / maxThrowIntensity, 1f) * throwAnimation.length)
                //make dashes

                MakeDashes(weightedThrowIntensity, pointerWorldPosition);
                //follow bait
                Follow(bait, followOffset);
            }
            yield return new WaitUntil(() => active);
            yield return new WaitForFixedUpdate();
        }
    }
    void ThrowAnim(float normalizedTime, bool follow = true) {
        throwAnimation.SampleAnimation(gameObject, normalizedTime * throwAnimation.length);

        if (follow)
        {
            //follow bait
            Follow(bait, followOffset);
        }
    }
    void LaunchPlane() 
    {
        //clear plane for launch
        plane.GetComponent<Rigidbody2D>().gravityScale = 1;
        plane.GetComponent<FlightControl>().enabled = true;
        plane.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        plane.GetComponent<PolygonCollider2D>().enabled = true;
        
        //play throw sound. TODO: rn we're assuming the throw sound is already set, improve this
        plane.GetComponent<AudioSource>().Play();

        //launch plane without exceeding max throw intensity
        if (throwVector.magnitude < maxThrowIntensity)
        {
            plane.GetComponent<FlightControl>().initialThrowImpulse = -throwVector;
            print($"go for launch, throw vector = {throwVector}");
        }
        else
        {
            float throwIntensity = throwVector.magnitude;
            Vector2 throwDirection = throwVector / throwIntensity;
            print($"go for launch at max, throw vector = {maxThrowIntensity * throwDirection}");
            plane.GetComponent<FlightControl>().initialThrowImpulse = -maxThrowIntensity * throwDirection;
            
        }
    }

    IEnumerator PreThrow()
    {
        while (true)
        {
            //yield return new WaitUntil(() => active);
            //print($"{gameObject.name}: PreThrow() oldThrowerCount: {oldThrowerCount}");
            if (active)
            {
                //print("init throw");
                plane.GetComponent<FlightControl>().thrust = 0;
                StartCoroutine(InitThrow());
                StartCoroutine(TauntThrow());
                //print("after taunt");
            }
            yield return new WaitUntil(() => thrown);
            //StopCoroutine(TauntThrow());
            //StopCoroutine(InitThrow());
            //print("thrown, waiting for deactivation");
            yield return new WaitUntil(()=> !active); //dependent on FPS by a little bit but we'll assume.
            //enabled = false;    
            yield return new WaitUntil(() => active);
        }
    }
    IEnumerator SleepALittle() 
    {
        yield return new WaitForSeconds(0.1f);
    }
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(PreThrow());
    }

    // Update is called once per frame
    void Update()
    {
        if (oldThrowerCount != newThrowerCount)
        {
            //print("prethrow should happen");
            oldThrowerCount++;
            StartCoroutine(PreThrow());
            //StartCoroutine(SleepALittle());
        }
    }
    IEnumerator OnMouseDown() {
        //launch plane if active
        thrown = true;
        if (usePoseLerp) 
        {
            gameObject.GetComponent<PoseLerp>().poseSequenceManager.play = false;
        }
       
        if (active) { 
            float animTime = 0;

            //keep physics off till launch
            //plane.GetComponent<FlightControl>().enabled = false;
            //plane.GetComponent<Rigidbody2D>().gravityScale = 0;
            //plane.GetComponent<PolygonCollider2D>().enabled = false;

            while (animTime <= 1-preLaunch) {
                ThrowAnim(animTime);
                animTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            LaunchPlane();
            while (animTime <= 1)
            {
                ThrowAnim(animTime, false);
                animTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            while (animTime >= 0)
            {
                ThrowAnim(animTime, false);
                animTime -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }
        Destroy(dashes);
        

        thrown = false;
        active = false;
    }
}
