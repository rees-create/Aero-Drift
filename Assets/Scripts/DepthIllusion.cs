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
    public bool popBackToRelativePosition;
    /// <summary>
    /// Set horizon to 0 to set the DepthIllusion to abstract and disable it.
    /// </summary>
    [SerializeField] float horizon;
    [SerializeField] float depth;
    public float parallaxFraction;
    public Color absoluteBlue;
    Vector3 distanceDifference;
    Vector3 previousPlayerPosition;
    void Follow(Vector3 playerVelocity, float speedFraction) 
    {
        //print($"follow params of {gameObject.name}: {Time.deltaTime}, {speedFraction}, {playerVelocity}");

        //print($"gameobject position {gameObject.transform.position}");
        gameObject.transform.position += playerVelocity * speedFraction * Time.deltaTime;

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
            distanceDifference = Vector3.Lerp(gameObject.transform.position, player.transform.position, playerRigidbody.velocity.magnitude * parallaxFraction);//gameObject.transform.position - player.transform.position;
            previousPlayerPosition = player.transform.position;
            //if (gameObject.GetComponent<ObjectSpawnSystem>().elementVariation.inReferenceFrame) 
            //{
            //    initialPosition = 
            //}
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (horizon != 0)
        {
            

            if (popBackToRelativePosition)
            {
                if (popBackController != null)
                {
                    Vector3 playerVelocity = (player.transform.position - previousPlayerPosition) * 1 / Time.deltaTime;
                    if (!popBackController.GetComponent<ObjectSpawnSystem>().popBack)
                    {
                        Follow(playerVelocity, parallaxFraction);
                    }
                    else
                    {
                        Follow(playerVelocity, 1);
                    }
                    previousPlayerPosition = player.transform.position;
                }
            }
            else 
            {
                Vector3 playerVelocity = (player.transform.position - previousPlayerPosition) * 1 / Time.deltaTime;
                Follow(playerVelocity, parallaxFraction);
                previousPlayerPosition = player.transform.position;
            }

            
        }
    }
}
