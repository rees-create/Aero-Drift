using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DepthIllusion : MonoBehaviour
{
    [SerializeField] GameObject player;
    Rigidbody2D playerRigidbody;
    Vector3 initialPosition;
    [SerializeField] GameObject popBackController;
    [SerializeField] float horizon;
    [SerializeField] float depth;
    [SerializeField] float parallaxFraction;
    [SerializeField] Color absoluteBlue;

    void Follow(Vector3 playerVelocity, float speedFraction) 
    {
        gameObject.transform.position += Time.deltaTime * speedFraction * playerVelocity;
    }
    public Vector4 ShiftBlue(Vector4 color) 
    {
        
        return Vector4.Lerp(color, absoluteBlue, parallaxFraction);
    }

    // Start is called before the first frame update
    void Start()
    {
        float horizonFraction = depth / horizon;
        float deg90 = Mathf.PI / 2;
        parallaxFraction = Mathf.Tan(deg90 * horizonFraction);
        playerRigidbody = player.GetComponent<Rigidbody2D>();

        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!popBackController.GetComponent<ObjectSpawnSystem>().popBack)
        {
            Follow(playerRigidbody.velocity, parallaxFraction);
        }
        else 
        {
            //pop back
            transform.position = initialPosition;
        }
    }
}
