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
        StartCoroutine(PlayCatchFollow(player.transform, followSpeed));
    }

    //Follow plane until plane is in catch radius while playing walking animation
    public IEnumerator PlayCatchFollow(Transform playerTransform, float speedFraction)
    {
        
        while (true) { 
            //Initializations
            int animTime = 0;
            float incomingSpeed = 0f;
            Vector2 incomingPosition = Vector2.zero;
            while (active)
            {
                // Follow player
                Vector3 playerVelocity = (playerTransform.position - transform.position).normalized;

                // Check distance to player
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (Mathf.Abs(distanceToPlayer - catchRadius) < 2)
                {
                    //disable physics
                    incomingSpeed = player.GetComponent<Rigidbody2D>().velocity.magnitude;
                    incomingPosition = player.transform.position;
                    player.GetComponent<PolygonCollider2D>().enabled = false;
                    player.GetComponent<Rigidbody2D>().gravityScale = 0;
                    //player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    player.GetComponent<FlightControl>().enabled = false;
                }

                if (distanceToPlayer <= catchRadius)
                {
                    //follow, catch plane and walk animations
                    Follow(player.GetComponent<Rigidbody2D>().velocity, speedFraction);
                    leftUpperCatch.SampleAnimation(gameObject, (leftUpperCatch.length / animFrameCount) * animTime);
                    Walk.SampleAnimation(gameObject, (leftUpperCatch.length / animFrameCount) * animTime);
                    // Move player to catch spot
                    Vector2 localCatchSpot = (Vector2) transform.position + catchSpot;
                    player.transform.position = Vector2.Lerp(incomingPosition, localCatchSpot, animTime / animFrameCount);
                    if (animTime == animFrameCount - 1)
                    {
                        // Reached catch radius, stop following
                        active = false;
                    }
                }



                animTime = (animTime + 1) % animFrameCount;
                yield return null; // Wait for next frame
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
