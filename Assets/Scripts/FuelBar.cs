using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FuelBar : MonoBehaviour
{
    public GameObject bar;
    public GameObject icon;
    public GameObject plane;
    public GameObject exhaust;
    Color initBarColor;
    public float fuelTime;
    public float barLength;
    //public bool addFuel;
    float remainingFuelTime;
    float initMaxThrust;
    bool outOfFuel;
    public void UpdateFuelBar()
    {
        if (!outOfFuel)
        {
            float thrustScale = plane.GetComponent<FlightControl>().thrust / plane.GetComponent<FlightControl>().maxThrust;
            remainingFuelTime -= Time.deltaTime * thrustScale;
            float t = 1 - (remainingFuelTime / fuelTime);
            bar.transform.localPosition = Vector3.Lerp(Vector3.zero, Vector3.left * barLength, t);
            float saturation = (initBarColor.maxColorComponent - Mathf.Min(initBarColor.r, Mathf.Min(initBarColor.g, initBarColor.b))) / initBarColor.maxColorComponent;
            bar.GetComponent<SpriteRenderer>().color = Color.Lerp(initBarColor, Color.HSVToRGB(0, saturation, initBarColor.grayscale), t);
            icon.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.HSVToRGB(0, 0.08f, 0.4f), t);
        }
    }
    public void OutOfFuel() 
    {
        outOfFuel = true;
        plane.GetComponent<FlightControl>().thrust = 0f;
        plane.GetComponent<FlightControl>().maxThrust = 0.01f;
        exhaust.GetComponent<ParticleSystem>().Stop();
    }

    public void AddFuel(float additionalFuelTime) 
    {
        outOfFuel = false;
        remainingFuelTime = (remainingFuelTime + additionalFuelTime) <= fuelTime ? remainingFuelTime + additionalFuelTime : fuelTime;
    }
    // Start is called before the first frame update
    void Start()
    {
        initBarColor = bar.GetComponent<SpriteRenderer>().color;
        initMaxThrust = plane.GetComponent<FlightControl>().maxThrust;
        remainingFuelTime = fuelTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (remainingFuelTime > 0f)
        {
            UpdateFuelBar();
        }
        if(remainingFuelTime <= 0f)
        {
            OutOfFuel();
        }
        else if (plane.GetComponent<FlightControl>().maxThrust == 0f)
        {
            plane.GetComponent<FlightControl>().maxThrust = initMaxThrust;
        }
    }
}
