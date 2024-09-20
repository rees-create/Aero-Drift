using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Plane
{
    public string name;
    public string spriteName;
    public string flightCurveType;
    public string AoAUnits;
    public float[] dragCurveCoefs;
    public float[] liftCurveCoefs;
    public float[] momentCurveCoefs;

    public override string ToString() 
    {
        return $"Plane [name: {name}, spriteName: {spriteName},\n" +
            $"flightCurveType: {flightCurveType} AoAUnits: {AoAUnits},\nliftCurveCoefs: {liftCurveCoefs}..]";
    }
}
