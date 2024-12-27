using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class Thrower : MonoBehaviour
{
    public GameObject plane;
    public GameObject dash;
    public Vector3 dashScale;
    public float dashQuotient;
    [System.NonSerialized] public GameObject dashes;
    Vector3 pointerPosition;
    Vector3 pointerWorldPosition;
    public float maxThrowIntensity;
    bool thrown = false;
    Vector2 throwVector;
    Vector3 initPlaneRotation;

    int nDashes = 0;

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
                //print($"delete at index {i}, childCount: {dashes.transform.childCount}");
                Destroy(dashes.transform.GetChild(i).gameObject);
                nDashes--;
                diff++;
            }
            //nDashes -= diff;
            //print($"nDashes = {nDashes}");
        }
        else if (diff > 0 && nDashes < 75) 
        { //more dashes
            int initChildCount = dashes.transform.childCount;
            for (int i = 0; i < diff; i++) 
            {
                float lerpFraction = numberOfDashes != 0 ? (float) (i + initChildCount) / (float) numberOfDashes : 1f;
                //print($"lerpFraction: {lerpFraction} diff: {diff} i = {i} numberOfDashes: {numberOfDashes}");
                GameObject _dash = Instantiate(dash, dashes.transform);
                _dash.transform.position = Vector2.Lerp(plane.transform.position, mousePosition, lerpFraction);
                _dash.transform.eulerAngles = currentRotation;
                dashes.transform.localScale = dashScale;
                nDashes++;
                diff--;
            }
            //nDashes += diff;
            
        }
        if (nDashes > 0) 
        {
            for (int i = 0; i < dashes.transform.childCount; i++)
            {
                float lerpFraction = numberOfDashes != 0 ? (float) i / (float) numberOfDashes : 1f;
                dashes.transform.GetChild(i).position = Vector2.Lerp(plane.transform.position, mousePosition, lerpFraction);
                dashes.transform.GetChild(i).eulerAngles = currentRotation;
                //print($"lerpFraction: {lerpFraction} diff: {diff} i = {i} numberOfDashes: {numberOfDashes} position: {dashes.transform.GetChild(i).position}");
            }
        }
        //print($"numberOfDashes: {numberOfDashes}, diff = {diff} plane position: {plane.transform.position}");

    }
    
    // Start is called before the first frame update
    void Start()
    {
        dashes = new GameObject ("Throw Dashes");
        dashes.transform.parent = transform;
        plane.GetComponent<FlightControl>().enabled = false;
        plane.GetComponent<Rigidbody2D>().gravityScale = 0;
        initPlaneRotation = plane.transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (!thrown)
        {
            pointerPosition = Input.mousePosition;
            //float throwLineRotation = Mathf.Atan
            pointerWorldPosition = Camera.main.ScreenToWorldPoint(pointerPosition);
            throwVector = plane.transform.position - pointerWorldPosition;
            float throwIntensity = throwVector.magnitude;
            //throwVector /= throwVector.magnitude;
       
            MakeDashes(throwIntensity, pointerWorldPosition);
        }
    }

    private void OnMouseDown()
    {
        //Debug.Log($"throwVector: {throwVector}, magnitude: {throwVector.magnitude}");
        thrown = true;
        plane.GetComponent<Rigidbody2D>().gravityScale = 1;
        plane.GetComponent<FlightControl>().enabled = true;
        
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
