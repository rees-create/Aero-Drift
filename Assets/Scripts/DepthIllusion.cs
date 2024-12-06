using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DepthIllusion : MonoBehaviour
{
    public GameObject player;
    Rigidbody playerRigidbody;
    public float horizon;
    public float depth;
    public float speedFraction;

    void follow(Vector3 playerVelocity, float speedFraction) 
    {
        gameObject.transform.position += Time.deltaTime * speedFraction * playerVelocity;
    }

    // Start is called before the first frame update
    void Start()
    {
        float horizonFraction = depth / horizon;
        float deg90 = Mathf.PI / 2;
        speedFraction = Mathf.Tan(deg90 * horizonFraction);
        playerRigidbody = player.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        follow(playerRigidbody.velocity, speedFraction);
    }
}
