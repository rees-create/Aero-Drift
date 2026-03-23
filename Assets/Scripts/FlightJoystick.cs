using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FlightJoystick : MonoBehaviour
{
    [SerializeField] bool setMiddleOnPlay;
    [SerializeField] bool useJoystickOnly;
    public FlightControl flightControl;
    public Vector2 middle;
    public GameObject pod;
    [SerializeField] float innerRadius;
    //TODO: use these values in FlightControl
    [NonSerialized] public FlightParams flightParams;
    [NonSerialized] public float throwIntensity;

    [SerializeField] int fixedFrameBuffer;
    public bool GetJoystickOnly() { return useJoystickOnly; }

    [Serializable]
    public struct FlightParams 
    {
        public float thrustMagnitude;
        public float flapAngle;
        public FlightParams(float thrustMagnitude, float flapAngle) { this.thrustMagnitude = thrustMagnitude; this.flapAngle = flapAngle; }
        
    }

    FlightParams InputToFlightParams(float magnitude) 
    {
        float angle = Vector2.Angle(middle, pod.transform.position);
        //height [sin(angle)] = flaps , width [cos(angle)] = thrust
        float normThrust = Mathf.Cos(angle * Mathf.Deg2Rad);
        float flaps = Mathf.Sin(angle * Mathf.Deg2Rad);

        return new FlightParams(normThrust * flightControl.maxThrust * magnitude, flaps * magnitude);
    }

    //inputtothrowparams
    

    bool MouseInRange() 
    {
        if (Vector2.Distance(Input.mousePosition, middle) <= innerRadius) 
        {
            return true;
        }
        return false;
    }

    float TrackPointer() 
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (MouseInRange())
            {
                pod.transform.position = Input.mousePosition;
                float toPointer = Vector2.Distance(Input.mousePosition, middle);
                return toPointer / innerRadius;
            }
            else
            {
                pod.transform.position = Input.mousePosition.normalized * innerRadius;
                return 1;
            }
        }
        else
        {
            pod.transform.position = middle;
            return 0;
        }
    }
    bool trackingPointer;
    public void StartTracking() //set this object to button onclick.
    {
        trackingPointer = true;
    }

    
    // Start is called before the first frame update
    void Start()
    {
        if (setMiddleOnPlay)
        {
            middle = pod.transform.position;
        }
        flightControl.SetUseJoystickOnly(useJoystickOnly);
    }
    int buffer = 0;
    // Update is called once per frame
    void FixedUpdate()
    {
        //if first click is inside the joystick, track pointer.

        //1.For throw, set magnitude to throw intensity
        throwIntensity = 0;
        if (trackingPointer) 
        {
            TrackPointer();
        }
        if (Input.GetMouseButtonUp(0)) 
        {
            throwIntensity = TrackPointer();
            //buffer then turn off tracking pointer;
            if (buffer <= fixedFrameBuffer)
            {
                buffer++;
            }
            else
            {
                trackingPointer = false;
            }
        }
        //2. For steering, set flight params
        flightParams = InputToFlightParams(TrackPointer());
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(middle, Vector3.right * innerRadius);
    }

}
