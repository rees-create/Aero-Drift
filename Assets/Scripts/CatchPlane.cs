using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchPlane : MonoBehaviour
{
    public bool active;
    public GameObject player;
    public float followSpeed;
    public int animFrameCount;
    //public Animator animator;
    //public float triggerName;

    //try playing animation clip independently of animator
    public AnimationClip Walk;
    public AnimationClip leftUpperCatch;
    //public string animationName;
    public float catchRadius;
    public Vector2 catchSpot;
    

    void Follow(Vector3 playerVelocity, float speedFraction)
    {
        //print($"follow params of {gameObject.name}: {Time.deltaTime}, {speedFraction}, {playerVelocity}");
        gameObject.transform.position += Time.deltaTime * speedFraction * playerVelocity;
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayCatchFollow(followSpeed));
    }

    //Follow plane until plane is in catch radius while playing walking animation
    public IEnumerator PlayCatchFollow(float speedFraction)
    {
        // TODO: Instead of waiting for catch radius trigger, have a wider read radius to monitor local catch spot and
        // prevent teleportation.
        while (true) { 
            //Initializations
            int animTime = 0;
            float incomingSpeed = 0f;
            Vector2 incomingPosition = player.transform.position;
            Vector2 localCatchSpot = (Vector2) transform.position + catchSpot;
            while (active)
            {
                // Follow player
                Vector3 playerVelocity = (player.transform.position - transform.position).normalized;

                // Check distance to player
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                if (Mathf.Abs(distanceToPlayer - catchRadius) < 2) // fulfil above TODO here. ?
                {
                    print("about at radius");
                    //disable physics
                    incomingSpeed = player.GetComponent<Rigidbody2D>().velocity.magnitude;
                    incomingPosition = player.transform.position;
                    player.GetComponent<PolygonCollider2D>().enabled = false;
                    player.GetComponent<Rigidbody2D>().gravityScale = 0;
                    //player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    player.GetComponent<FlightControl>().enabled = false;
                    localCatchSpot = (Vector2) transform.position + catchSpot;
                    
                }

                if (distanceToPlayer <= catchRadius)
                {
                    print("inside radius");
                    //follow, catch plane and walk animations
                    Follow(player.GetComponent<Rigidbody2D>().velocity, speedFraction);
                    leftUpperCatch.SampleAnimation(gameObject, (leftUpperCatch.length / animFrameCount) * animTime);
                    Walk.SampleAnimation(gameObject, (Walk.length / animFrameCount) * animTime);

                    //these must be off here too!
                    player.GetComponent<PolygonCollider2D>().enabled = false;
                    player.GetComponent<Rigidbody2D>().gravityScale = 0;
                    player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    player.GetComponent<FlightControl>().enabled = false;
                    localCatchSpot = (Vector2)transform.position + catchSpot; //think this should be here too
                    // Move player to catch spot
                    
                    player.transform.position = (Vector3) Vector2.Lerp(incomingPosition, localCatchSpot, (float) animTime / (float) animFrameCount);
                    //print($"where i want it: {Vector2.Lerp(incomingPosition, catchSpot, animTime / animFrameCount)}, where it is: {player.transform.position}, animFraction: {(float)animTime / (float)animFrameCount}");
                    if (animTime == animFrameCount - 1)
                    {
                        // Reached catch radius, stop following
                        player.GetComponent<PolygonCollider2D>().enabled = false;
                        player.GetComponent<Rigidbody2D>().gravityScale = 0;
                        player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                        player.GetComponent<FlightControl>().enabled = false;
                        active = false;
                    }
                    animTime = (animTime + 1) % animFrameCount;
                }



                
                yield return new WaitForEndOfFrame(); // Wait for next frame
            }
            yield return new WaitUntil(() => active);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Draw gizmo for catch radius
    void OnDrawGizmos() 
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, catchRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere((Vector2)transform.position + catchSpot, 0.5f);
    }
}
