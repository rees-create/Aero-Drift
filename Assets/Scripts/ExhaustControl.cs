using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class ExhaustControl : MonoBehaviour
{
    [SerializeField] FlightControl flightControl;
    [Header("Emission")]
    [SerializeField] int maxRateOverTime;
    [Header("Exhaust Velocity")]
    [SerializeField] float exhaustSpeed;
    void ControlExhaust() {

        if (!GetComponent<ParticleSystem>() || flightControl == null) 
        {
            return;
        }
        // Emission
        float thrustInput = flightControl.thrust;
        ParticleSystem.EmissionModule emission = GetComponent<ParticleSystem>().emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(maxRateOverTime * thrustInput);
        // Velocity over Lifetime
        ParticleSystem.VelocityOverLifetimeModule velocity = GetComponent<ParticleSystem>().velocityOverLifetime;
        velocity.z = -exhaustSpeed * thrustInput;
        //velocity.speedModifierMultiplier = exhaustSpeed/2;
        //Emitter rotation
        ParticleSystem.ShapeModule shape = GetComponent<ParticleSystem>().shape;
        shape.rotation.Set(shape.rotation.x, shape.rotation.y, transform.TransformDirection(transform.rotation.eulerAngles).z);
    }

    int activateExhaustCounter = 0;
    void ActivateExhaust() 
    {
        if (activateExhaustCounter > 1 || GetComponent<ParticleSystem>() == null)
        {
            activateExhaustCounter = 0;
            return;
        }
        GetComponent<ParticleSystem>().Play();
    }
    int StopExhaustCounter = 0;
    void StopExhaust() 
    {
        if (StopExhaustCounter > 1 || GetComponent<ParticleSystem>() == null) {
            StopExhaustCounter = 0;
            return;
        }
        GetComponent<ParticleSystem>().Stop();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (flightControl.thrust > 0.001) 
        {
            ActivateExhaust();
            ControlExhaust();
        }
        else
        {
            StopExhaust();
        }   
    }
}
