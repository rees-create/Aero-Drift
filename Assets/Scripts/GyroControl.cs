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
    public Quaternion gyroRotation;
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
        float gyroX = gyroRotation.eulerAngles.x;
        if (gyroX >= flapRanges.zeroUpper && gyroX < 270)
        {
            float range = flapRanges.upperBound - flapRanges.zeroUpper;
            float xDiff = gyroX - flapRanges.zeroUpper;
            float flapFraction = range / xDiff;
            return flapFraction <= 1 ? flapFraction : 1;
        }
        else if (gyroX < flapRanges.zeroUpper && gyroX > flapRanges.zeroLower)
        {
            return 0;
        }
        else
        {
            if (gyroX > 270)
            {
                gyroX = gyroX - 360;
            }
            float range = flapRanges.zeroLower - flapRanges.lowerBound;
            float xDiff = gyroX - flapRanges.zeroUpper;
            float flapFraction = range / xDiff;
            return flapFraction >= -1 ? flapFraction : -1;
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
            gyroRotation = gyro.attitude;
            //print(gyroRotation.eulerAngles);
        }
        
    }
}
