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
    [SerializeField] float outerRadius;
    //TODO: use these values in FlightControl
    [NonSerialized] public FlightParams flightParams;
    public float throwIntensity;

    [SerializeField] int fixedFrameBuffer;

    bool mouseDown = false;
    Vector2 throwJoystickSnapshot;

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
        float angle = Vector2.SignedAngle(Vector2.right, (Vector2) pod.transform.localPosition - middle);
        //height [sin(angle)] = flaps , width [cos(angle)] = thrust
        float normThrust = Mathf.Cos(angle * Mathf.Deg2Rad);
        float flaps = Mathf.Sin(angle * Mathf.Deg2Rad);
        //print("flaps = " + flaps);
        return new FlightParams(normThrust * flightControl.maxThrust * magnitude, flaps, throwJoystickSnapshot * throwIntensity);
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
    public bool MouseOnJoystick() 
    {
        Vector2 localMousePosition = transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (Vector2.Distance(localMousePosition, middle) <= outerRadius)
        {
            return true;
        }
        return false;
    }
    bool trackingPointer;

    public float TrackPointer(bool hover = false) 
    {
        mouseDown = Input.GetMouseButton(0);
        //bool mouseDownXorHover = (mouseDown || hover) && (!(mouseDown && hover));
        if (MouseOnJoystick() && mouseDown)
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
        if (MouseOnJoystick() && Input.GetMouseButton(0))
        {
            joystickMagnitude = TrackPointer();
            //print("trackingPointer = " + trackingPointer);
            //throwIntensity = joystickMagnitude;//TrackPointer();
            throwJoystickSnapshot = joystickPos;
            
            flightParams = InputToFlightParams(joystickMagnitude);
            //print("throw impulse: " + flightParams.throwImpulse);
        }
        else //(!Input.GetMouseButton(0)) 
        {
            //throwIntensity = joystickMagnitude;
            //buffer then turn off tracking pointer;
            if (buffer <= fixedFrameBuffer)
            {
                buffer++;
                flightParams = InputToFlightParams(joystickMagnitude);
            }
            else
            {
                buffer = 0;
                //throwIntensity = 0;
                trackingPointer = false;
                //joystickMagnitude = 0;
                
                //print("idle throw impulse: " + flightParams.throwImpulse);
            }
        }


        //2. For steering, set flight params
        if (flightControl.enabled)
        {
            flightParams = InputToFlightParams(joystickMagnitude);
            //print("flight control off");
        }
        
        //print("joystickMagnitude = " + joystickMagnitude);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.TransformPoint(middle), transform.TransformPoint(Vector3.right * innerRadius));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.TransformPoint(middle), transform.TransformPoint(Vector3.up * outerRadius));
    }

}
