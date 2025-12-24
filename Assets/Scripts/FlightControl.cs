using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FlightControl : MonoBehaviour
{
    Rigidbody2D rb;
    List<Vector2> forces;
    [SerializeField] SpriteRenderer spriteRenderer;
    float length;
    [Header("Scale and Kinematic Properties")]
    [SerializeField] Vector2 dimensions;
    public Vector2 centerOfMass;
    [SerializeField] float centerOfMassGizmoRadius;
    [SerializeField] float wingArea;
    [SerializeField] Range chord;
    [Header("Wind Field Interaction")]
    public Vector2 wind;
    [Header("Other Adjustables")]
    [SerializeField] float airDensity;
    [SerializeField] float liftCoefficientRange;
    [SerializeField] float dragCoefficientRange;
    [SerializeField] float momentCoefficientRange;
    [Header("Flap Control")]
    [SerializeField] float flapSpeed;
    [SerializeField] float flapInfluence;
    [SerializeField] bool flapsDirectlyIncreaseLift;
    [Header("Impetus Force")]
    public Vector2 initialThrowImpulse;
    public float maxThrust;
    [Header("Plane Specifications")]
    public TextAsset planeSpecsFile;
    public int planeIndex;
    [Header("Exploding Force Guard")]
    [SerializeField] float maxAllowedDifferential;
    [SerializeField] float tangentialVelocityDivisor = 9.8f;

    [System.Serializable]
    struct Range {
        public float min, max;
    }
    [Header("Flap Data")]
    public float AoA;
    [SerializeField] float flapAngle;
    [SerializeField] Vector2 flapDrag;
    [SerializeField] Vector2 flapLift;
    [SerializeField] float flapTorque;
    
    Plane plane;
    Vector2[] forceLog = new Vector2[2];
    float[] torqueLog = new float[2];
    int t_ = 0;

    float AeroForce(float rho, Vector2 velocity, float area, float C, float maxC = 0) 
    {
        //Vector2 vsquared = new Vector2(Mathf.Pow(velocity.x, 2), Mathf.Pow(velocity.y, 2));
        float vsquared = Mathf.Pow(velocity.magnitude, 2);
        float magnitudeNoC = 0.5f * rho * vsquared * area;
        return magnitudeNoC * C;
    }

    //Low speed/pre equilibrium: nose force < back wing force
    //Glide speed/equilibrium: nose force = back wing force 
    //High speed/post equilibrium: nose force > back wing force
    //Use AddForceAtPosition
    List<Vector3> BalancedForce(Vector3 moment, Vector3 totalForce, float length, Vector3 frontLever) 
    {
        float partition = length / frontLever.magnitude;
        Vector3 partitionedForce = totalForce * partition;
        Vector3 partitionedPivot = Vector3.Cross(moment, frontLever) * partition;//moment * partition; //idek if this name makes sense
        Vector3 backForce = partitionedForce - partitionedPivot;
        Vector3 frontForce = totalForce - backForce;

        return new List<Vector3>() { backForce, frontForce };
    }
    List<Vector2> BalancedForceConstrained(float moment, Vector2 totalForce, float length, float frontLever) 
    {
        //float theta = Mathf.Atan2(totalForce.y, totalForce.x);
        Vector2 backForce = (frontLever * totalForce - new Vector2(moment, moment)) / length;      
        Vector2 frontForce = totalForce - backForce;

        if (backForce.magnitude > totalForce.magnitude)
        {
            //print("invalid force detected");
        }
        if (backForce + frontForce != totalForce) 
        {
            //print("backForce + frontForce != totalForce");
        }

        return new List<Vector2>() { backForce, frontForce };
    }
    // Next steps
    // TODO: get curves
    // TODO: write PlaneSpecs.json file with sprite info, flight splines, etc
    struct Bounds 
    {
        public float lower, higher;
        public Bounds(float lower, float higher) 
        {
            this.lower = lower;
            this.higher = higher;
        }
        //public Bounds() { }
    }
    float PolynomialCurve(float[] coefs, float x) 
    {
        float value = 0;
        
        for (int i = 0, power = coefs.Length - 1; i < coefs.Length; i++, power--) 
        {
            value += coefs[i] * Mathf.Pow(x, power); // valueDenominator;
        }
        return value;
    }
    Plane ReadPlaneData(int planeIndex) 
    {
        //TODO: read coefficients from JSON.. may not need this function though
        Planes planesInJSON = JsonUtility.FromJson<Planes>(planeSpecsFile.text);
        return planesInJSON.planes[planeIndex];
    }
    /// <summary>
    /// Gets appropriate coefficients for a given AoA from the full list
    /// </summary>
    /// <param name="curveCoefs"></param>
    /// <param name="AoA"></param>
    /// <param name="nCurveCoefs"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    float[] Coefs(float[] curveCoefs, float AoA, int nCurveCoefs, float range, float flapValue = 0) 
    {
        float quad = Mathf.PI / 2;
        int coefStart = (int) Mathf.Floor(AoA / quad) * nCurveCoefs;
       
        //get coefficients first
        float[] coefs = new float[4];
        for (int i = 0, j = coefStart; i < nCurveCoefs; i++, j++) 
        {
            coefs[i] = curveCoefs[j];
        }
        //add flap value to bias (last) coefficient
        coefs[3] += flapValue;
        //calculate range, value denominator
        Bounds bounds = new Bounds();
        float quadrant = coefStart / nCurveCoefs;
        bounds.lower = PolynomialCurve(coefs, quadrant * quad);
        bounds.higher = PolynomialCurve(coefs, ((quadrant + quad) * quad) % (quad * 4));
        //print($"Bounds: ({bounds.lower}, {bounds.higher})");
        float initRange = Mathf.Abs(bounds.higher - bounds.lower);
        float valueDenominator = initRange / range;
        //loop through coefs, divide by value denominator (if applicable) to constrain to range
        if (valueDenominator > 1)
        {
            for (int i = 0; i < nCurveCoefs; i++)
            {
                coefs[i] /= valueDenominator;
            }
        }

        return coefs;
    }
    List<Vector2> AeroUpdate(Plane plane, float length, float thrustScalar) 
    {
        
        //AoA calculation from dot product of velocity and orientation
        AoA = 0;
        float localBack = -length / 2;
        float localFront = length / 2;
        float frontLever = new Vector2(localFront - rb.centerOfMass.x, localFront - rb.centerOfMass.y).magnitude;
        //Vector2 localVelocity = Vector2.zero;
        float acrossChord = Mathf.Max(rb.centerOfMass.x - chord.max, rb.centerOfMass.x - chord.min);
        float chordLength = chord.max - chord.min;
        float chordSign = acrossChord / Mathf.Abs(acrossChord);
        Vector2 tangentialVelocity = new Vector2(0, rb.angularVelocity * Mathf.Deg2Rad * chordLength * chordSign);

        Vector2 airspeed = rb.velocity + wind - (tangentialVelocity * (1 / tangentialVelocityDivisor));
        if (rb.velocity.magnitude > 0)
        {
            Vector2 localVelocity = transform.InverseTransformDirection(airspeed);
            //float dotprod = Vector2.Dot(localVelocity, Vector2.right);
            //AoA = Mathf.Acos(dotprod / localVelocity.magnitude);
            AoA = Vector2.SignedAngle(localVelocity, Vector2.right) * Mathf.Deg2Rad;
            if (AoA < 0) 
            {
                AoA = (2 * Mathf.PI) + AoA;
            }
        }
        
        float[] dragCurveCoefs = Coefs(plane.dragCurveCoefs, AoA, 4, dragCoefficientRange, FlapValue(flapInfluence)); //we only have cubic, so I just put 4 there. 
        float[] liftCurveCoefs = Coefs(plane.liftCurveCoefs, AoA, 4, liftCoefficientRange, FlapValue(flapInfluence));
        float[] momentCurveCoefs = Coefs(plane.momentCurveCoefs, AoA, 4, momentCoefficientRange, FlapValue(flapInfluence)/10);


        float Cd = Mathf.Abs(PolynomialCurve(dragCurveCoefs, AoA));
        float Cl = PolynomialCurve(liftCurveCoefs, AoA);
        float Cm = PolynomialCurve(momentCurveCoefs, AoA);

        float lift = AeroForce(airDensity, airspeed, wingArea, Cl, liftCoefficientRange);
        float drag = AeroForce(airDensity, airspeed, wingArea, Cd, dragCoefficientRange);

        Vector2 downwind = airspeed.magnitude > 0 ? - airspeed / airspeed.magnitude : Vector2.zero; //unit vector downwind
        Vector2 liftDir = Vector3.Cross(downwind, Vector3.forward);
        Vector2 liftForce = liftDir * lift;
        Vector2 dragForce = downwind * drag;
        Vector2 thrustForce = - downwind * thrustScalar;
        float torque = AeroForce(airDensity, airspeed, wingArea, Cm, momentCoefficientRange) * chordLength;
        //KeyValuePair<Vector2, float> flapImpetus = KeyboardFlapControl(dragForce, liftForce, torque, Cd, Cl);
        //torque += flapImpetus.Value;

        Vector2 totalForce = liftForce + dragForce + thrustForce; //+ flapImpetus.Key;


        
        
       
        //print(torque);
        //if (totalForce.magnitude > rb.mass * 9.81f) //constrain all force to be under gravity
        //{ //instead restrict high force differentials
        //    totalForce /= totalForce.magnitude;
        //    totalForce *= rb.mass * 9.81f;
        //}
        
        forceLog[t_] = totalForce;
        torqueLog[t_] = torque;
        t_ = (t_ + 1) % 2;
        if ((forceLog[1] - forceLog[0]).magnitude > maxAllowedDifferential) 
        {
            totalForce = Vector2.zero;
        }
        if (torqueLog[1] - torqueLog[0] > (maxAllowedDifferential / (chord.max-chord.min))) 
        {
            torque = 0;
        }

        rb.AddForce(totalForce);
        rb.AddTorque(torque);

        List<Vector2> balancedForce = BalancedForceConstrained(torque, totalForce, length, frontLever);
        
        return balancedForce;
    }

    float FlapValue(float influence) {
        float degree = Mathf.PI / 180;
        if (Input.GetKey(KeyCode.UpArrow) && flapAngle < Mathf.PI / 2)
        {
            flapAngle += degree * flapSpeed;
        }
        if (Input.GetKey(KeyCode.DownArrow) && flapAngle > -Mathf.PI / 2)
        {
            flapAngle -= degree * flapSpeed;
        }
        float flapFraction = flapAngle / (Mathf.PI / 2);
        return flapFraction * influence;
    }

    float thrust = 0;
    float ApplyThrust(float maxThrust) 
    {
        
        float increment = 0.01f;
        if (Input.GetKey(KeyCode.RightArrow) && thrust <= maxThrust)
        {
            thrust += increment;
        }
        if (Input.GetKey(KeyCode.LeftArrow) && thrust >= 0)
        {
            thrust -= increment;
        }
        return thrust;
    }

    KeyValuePair<Vector2, float> KeyboardFlapControl(Vector2 drag, Vector2 lift, float torque, float Cd, float Cl) 
    {
        float degree = Mathf.PI / 180;
        if (Input.GetKey(KeyCode.UpArrow) && flapAngle < Mathf.PI / 2) 
        {
            flapAngle += degree * flapSpeed;
        }
        if (Input.GetKey(KeyCode.DownArrow) && flapAngle > - Mathf.PI / 2) 
        {
            flapAngle -= degree * flapSpeed;
        }
        float flapFraction = flapAngle / (Mathf.PI / 2);
        float liftToDragRatio = Cl / Cd;
        if (lift.magnitude != 0 && drag.magnitude != 0 && torque != 0)
        {
            flapDrag = ((drag * Mathf.Abs(flapFraction) * flapInfluence) / liftToDragRatio);
            if (flapsDirectlyIncreaseLift) 
            {
                flapLift = ((lift * flapFraction * flapInfluence) / liftToDragRatio);
            }
            flapTorque = Mathf.Abs(torque * flapInfluence) * flapFraction / liftToDragRatio;
            
            return new KeyValuePair<Vector2, float>(flapDrag + flapLift, flapTorque);
        }
        return new KeyValuePair<Vector2, float>(Vector2.zero, 0);
    }

    //IEnumerator SetImpulseCoroutine() 
    //{
    //    while (true) 
    //    {
    //        rb.AddForce(initialThrowImpulse, ForceMode2D.Impulse);
    //        initialThrowImpulse = 0;
    //        yield return new WaitForSeconds(1f);

    //    }
    //}
   
    // Start is called before the first frame update
    void Start()
    {
        //spriteRenderer = GetComponent<SpriteRenderer>();
        length = dimensions.x;
        rb = GetComponent<Rigidbody2D>();
        plane = ReadPlaneData(planeIndex);
        rb.centerOfMass = centerOfMass;
        //rb.AddForce(initialThrowImpulse, ForceMode2D.Impulse);

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(centerOfMass), centerOfMassGizmoRadius);
        Vector2 inPlayFront = length == 0 ? dimensions/2 : new Vector2(length/2, 0);
        
        //Gizmos.DrawCube(transform.TransformPoint(inPlayFront), new Vector3(0.4f, 0.4f, 0.4f));
        
        if (spriteRenderer != null)
        {
            Vector2 localBack = Vector2.left * dimensions.x/2;
            Vector2 localFront = Vector2.right * dimensions.x/2;
            if (forces != null && forces.Count > 0)
            {
                Gizmos.DrawLine(transform.TransformPoint(localBack), transform.TransformPoint(localBack + forces[0]));
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.TransformPoint(localFront), transform.TransformPoint(localFront + forces[1]));
            }
        }
        
    }

    void Update()
    {
        if(initialThrowImpulse != Vector2.zero)
        {
            rb.AddForce(initialThrowImpulse, ForceMode2D.Impulse);
            initialThrowImpulse = Vector2.zero;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        
        forces = AeroUpdate(plane, length, ApplyThrust(maxThrust));
       
    }
}
