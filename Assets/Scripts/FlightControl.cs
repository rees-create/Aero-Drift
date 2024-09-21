using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightControl : MonoBehaviour
{
    Rigidbody2D rb;
    List<Vector2> forces;
    SpriteRenderer spriteRenderer;
    [SerializeField] float airDensity;
    [SerializeField] float wingArea;
    [SerializeField] float centerOfMassGizmoRadius;
    public TextAsset planeSpecsFile;
    public int planeIndex;
    public Vector2 centerOfMass;
    
    //[SerializeField] float length;
    Plane plane;

    float AeroForce(float rho, Vector2 velocity, float area, float C) 
    {
        //Vector2 vsquared = new Vector2(Mathf.Pow(velocity.x, 2), Mathf.Pow(velocity.y, 2));
        float vsquared = Mathf.Pow(velocity.magnitude, 2);
        return 0.5f * rho * vsquared * area * C;
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
        Vector2 backForce = (frontLever * totalForce - new Vector2(moment, moment)) / length;
        Vector2 frontForce = totalForce - backForce;
        return new List<Vector2>() { frontForce, backForce };
    }
    // Next steps
    // TODO: get curves
    // TODO: write PlaneSpecs.json file with sprite info, flight splines, etc
    float PolynomialCurve(float[] coefs, float x) 
    {
        float value = 0;
        for (int i = coefs.Length - 1; i >= 0; i--) 
        {
            value += coefs[i] * Mathf.Pow(x, i);
        }
        return value;
    }
    Plane ReadPlaneData(int planeIndex) 
    {
        //TODO: read coefficients from JSON.. may not need this function though
        Planes planesInJSON = JsonUtility.FromJson<Planes>(planeSpecsFile.text);
        return planesInJSON.planes[planeIndex];
    }
    float[] Coefs(float[] curveCoefs, float AoA, int nCurveCoefs) 
    {
        float quad = Mathf.PI / 2;
        int coefStart = (int) Mathf.Floor(AoA / quad) * nCurveCoefs;
        float[] coefs = new float[4];
        for (int i = 0, j = coefStart; i < nCurveCoefs; i++, j++) 
        {
            coefs[i] = curveCoefs[j]; 
        }
        return coefs;
    }
    List<Vector2> AeroUpdate(Plane plane) 
    {
        //AoA calculation from dot product of velocity and orientation
        Vector2 orientation = transform.TransformPoint(Vector2.right);
        float dotprod = Vector2.Dot(rb.velocity, orientation);
        float AoA = Mathf.Acos(dotprod / (rb.velocity.magnitude * orientation.magnitude));

        float[] dragCurveCoefs = Coefs(plane.dragCurveCoefs, AoA, 4); //we only have cubic, so I just put 4 there. 
        float[] liftCurveCoefs = Coefs(plane.liftCurveCoefs, AoA, 4);
        float[] momentCurveCoefs = Coefs(plane.momentCurveCoefs, AoA, 4);

        float Cd = PolynomialCurve(dragCurveCoefs, AoA);
        float Cl = PolynomialCurve(liftCurveCoefs, AoA);
        float Cm = PolynomialCurve(momentCurveCoefs, AoA);

        float lift = AeroForce(airDensity, rb.velocity, wingArea, Cl);
        float drag = AeroForce(airDensity, rb.velocity, wingArea, Cd);

        Vector2 downwind = - rb.velocity / rb.velocity.magnitude; //unit vector downwind
        Vector2 liftDir = Vector3.Cross(downwind, Vector3.forward);

        Vector2 totalForce = (liftDir * lift) + (downwind * drag);
        //float totalForce = lift + drag; //don't do this
       
        float torque = AeroForce(airDensity, rb.velocity, wingArea, Cm);
        float localBack =  -spriteRenderer.bounds.size.x/2;
        float localFront = spriteRenderer.bounds.size.x/2;

        List<Vector2> balancedForce = BalancedForceConstrained(torque, totalForce, spriteRenderer.bounds.size.x, localFront - rb.centerOfMass.x);

        rb.AddForceAtPosition(balancedForce[0], new Vector2(localFront, 0));
        rb.AddForceAtPosition(balancedForce[1], new Vector2(localBack, 0));

        return balancedForce;
    }

   
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        plane = ReadPlaneData(planeIndex);
        rb.centerOfMass = centerOfMass;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(centerOfMass), centerOfMassGizmoRadius);
        
        if (spriteRenderer != null)
        {
            Vector2 localBack = Vector2.left * spriteRenderer.bounds.size.x/2;
            Vector2 localFront = Vector2.right * spriteRenderer.bounds.size.x/2;
            if (forces.Count > 0)
            {
                Gizmos.DrawLine(transform.TransformPoint(localBack), transform.TransformPoint(localBack + forces[0]));
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.TransformPoint(localFront), transform.TransformPoint(localFront + forces[1]));
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        forces = AeroUpdate(plane);
    }
}
