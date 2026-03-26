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
    [NonSerialized] public Vector2 joystickPos;
    [SerializeField] float innerRadius;
    //TODO: use these values in FlightControl
    [NonSerialized] public FlightParams flightParams;
    [NonSerialized] public float throwIntensity;

    [SerializeField] int fixedFrameBuffer;

    bool mouseDown = false;

    public bool GetJoystickOnly() { return useJoystickOnly; }

    [Serializable]
    public struct FlightParams 
    {
        public float thrustMagnitude;
        public float flapFraction;
        public Vector2 throwImpulse;
        public FlightParams(float thrustMagnitude, float flapFraction, Vector2 throwImpulse) 
        {
            this.thrustMagnitude = thrustMagnitude; 
            this.flapFraction = flapFraction; 
            this.throwImpulse = throwImpulse;
        }
        
    }

    FlightParams InputToFlightParams(float magnitude) 
    {
        float angle = Vector2.Angle(Vector2.zero, (Vector2) pod.transform.localPosition - middle);
        //height [sin(angle)] = flaps , width [cos(angle)] = thrust
        float normThrust = Mathf.Cos(angle * Mathf.Deg2Rad);
        float flaps = Mathf.Sin(angle * Mathf.Deg2Rad);
        print("normThrust = " + normThrust);
        return new FlightParams(normThrust * flightControl.maxThrust * magnitude, flaps * magnitude, joystickPos * flightControl.maxThrust);
    }

    //inputtothrowparams
    

    bool MouseInRange() 
    {
        Vector2 localMousePosition = transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (Vector2.Distance(localMousePosition, middle) <= innerRadius) 
        {
            return true;
        }
        return false;
    }

    float TrackPointer() 
    {
        mouseDown = Input.GetMouseButton(0);
        if (mouseDown)
        {
            //print("mouse down");
            Vector2 localMousePosition = transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (MouseInRange())
            {
                pod.transform.localPosition = localMousePosition;
                float toPointer = Vector2.Distance(localMousePosition, middle);
                joystickPos = (localMousePosition - middle) / innerRadius;
                return toPointer / innerRadius;
            }
            else
            {
                pod.transform.localPosition = localMousePosition.normalized * innerRadius;
                joystickPos = (localMousePosition-middle).normalized;
                return 1;
            }
        }
        else
        {
            
            pod.transform.localPosition = middle;
            joystickPos = Vector2.zero;
            //print("mouse up");
            mouseDown = false;
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
            middle = pod.transform.localPosition;
        }
        flightControl.SetUseJoystickOnly(useJoystickOnly);
    }
    int buffer = 0;
    
    // Update is called once per frame
    void FixedUpdate()
    {
        //if first click is inside the joystick, track pointer.

        //1.For throw, set magnitude to throw intensity
        float joystickMagnitude = TrackPointer();
        if (trackingPointer) 
        {
            
            throwIntensity = joystickMagnitude;//TrackPointer();
        }
        if (Input.GetMouseButton(0)) 
        {
            throwIntensity = joystickMagnitude;
            //buffer then turn off tracking pointer;
            if (buffer <= fixedFrameBuffer)
            {
                buffer++;
            }
            else
            {
                buffer = 0;
                throwIntensity = 0;
                trackingPointer = false;
            }
        }
        //2. For steering, set flight params
        flightParams = InputToFlightParams(joystickMagnitude);
        //print("joystickMagnitude = " + joystickMagnitude);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.TransformPoint(middle), transform.TransformPoint(Vector3.right * innerRadius));
    }

}
