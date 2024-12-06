using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindField : MonoBehaviour
{
    public Vector2 wind;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D collider) 
    {
        if (collider.gameObject.GetComponent<FlightControl>() != null)
        {
            collider.gameObject.GetComponent<FlightControl>().wind = wind;
        }
    }
    void OnTriggerExit2D(Collider2D collider) 
    {
        if (collider.gameObject.GetComponent<FlightControl>() != null)
        {
            collider.gameObject.GetComponent<FlightControl>().wind = Vector2.zero;
        }
    }
}
