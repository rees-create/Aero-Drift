using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class GyroControl : MonoBehaviour
{
    bool gyroEnabled;
    Gyroscope gyro;
    public Vector3 gyroRotation;
    [Serializable]
    public struct FlapRanges 
    {
        public float upperBound, zeroUpper, zeroLower, lowerBound;
    }
    [SerializeField] FlapRanges flapRanges;
    FlightControl flightControl;

    bool SupportsGyro() 
    {
        if (SystemInfo.supportsGyroscope) 
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            return true;
        }
        return false;
    }

    public float FlapFraction() 
    {
        float gyroY = gyroRotation.y;
        if (gyroY >= flapRanges.zeroUpper && gyroY < 270)
        {
            float range = flapRanges.upperBound - flapRanges.zeroUpper;
            float yDiff = gyroY - flapRanges.zeroUpper;
            float flapFraction = yDiff / range;
            print($"flapFraction: {flapFraction}");
            return flapFraction <= 1f ? flapFraction : 1f;
        }
        else if (gyroY < flapRanges.zeroUpper && gyroY > flapRanges.zeroLower)
        {
            return 0;
        }
        else
        {
            if (gyroY > 270)
            {
                gyroY = gyroY - 360;
            }
            float range = flapRanges.zeroLower - flapRanges.lowerBound;
            float yDiff = gyroY - flapRanges.zeroLower;
            float flapFraction = yDiff / range;
            print($"flapFraction: {flapFraction}");
            return flapFraction >= -1f ? flapFraction : -1f;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        gyroEnabled = SupportsGyro();
    }

    // Update is called once per frame
    void Update()
    {
        if (SupportsGyro()) 
        {
            gyroRotation = gyro.attitude.eulerAngles;
            //print(gyroRotation.eulerAngles);
        }
        
    }
}
