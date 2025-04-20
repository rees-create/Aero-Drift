using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DepthIllusion : MonoBehaviour
{
    public GameObject player;
    Rigidbody2D playerRigidbody;
    Vector3 initialPosition;
    public GameObject popBackController;
    /// <summary>
    /// Set horizon to 0 to set the DepthIllusion to abstract and disable it.
    /// </summary>
    [SerializeField] float horizon;
    [SerializeField] float depth;
    public float parallaxFraction;
    public Color absoluteBlue;

    void Follow(Vector3 playerVelocity, float speedFraction) 
    {
        //print($"follow params of {gameObject.name}: {Time.deltaTime}, {speedFraction}, {playerVelocity}");
        gameObject.transform.position += Time.deltaTime * speedFraction * playerVelocity;
    }
    

    // Start is called before the first frame update
    void Start()
    {
        //if horizon is 0 this depth illusion is abstract - meaning it is disabled and only used as a DepthIllusion data access point for 
        //child objects
        if (horizon != 0) 
        {
            float horizonFraction = depth / horizon;
            float deg90 = Mathf.PI / 2;
            parallaxFraction = Mathf.Sin(deg90 * horizonFraction);
            playerRigidbody = player.GetComponent<Rigidbody2D>();
            initialPosition = transform.position;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (horizon != 0)
        {
            //print(playerRigidbody);
            if (!popBackController.GetComponent<ObjectSpawnSystem>().popBack)
            {
                Follow(player.GetComponent<Rigidbody2D>().velocity, parallaxFraction);
                //print($"rigidbody velocity: {playerRigidbody.velocity}, parallaxFraction {parallaxFraction}, product = {player.GetComponent<Rigidbody2D>().velocity * parallaxFraction}");
                
            }
            else
            {
                //pop back
                transform.position = initialPosition;
            }
            
        }
    }
}
