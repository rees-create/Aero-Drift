using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pusher : MonoBehaviour
{
    
    public Animator animator;
    public string animationName;
    public float pushSpeed;
    public Vector2 target;
    public float slowDownAt;
    public float slowDownDamping;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Play());
    }
    // Add play coroutine. Coroutine should play movement frames and wait for animation.
    IEnumerator Play() {
        while (true) //in the grand scheme use a game over condition.
        {
            Vector2 initialPosition = transform.position;
            while (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            {
                float time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                Push(time, initialPosition, target, slowDownAt, slowDownDamping);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(animationName));
        }
    }
    
    void Push(float time, Vector2 initialPosition, Vector2 target, float slowDownProximityPercent, float slowDownDamping)
    { 
        float fullSpeedPercent = 1 - slowDownProximityPercent;
        if (time < fullSpeedPercent)
        {
            //go towards destination
            transform.position = Vector2.Lerp(initialPosition, target, time);
        }
        else
        {
            //slow down as you approach destination
            Vector2 slowDownPoint = Vector2.Lerp(initialPosition, target, fullSpeedPercent);
            Vector2 movingStartPoint = Vector2.Lerp(target - slowDownPoint, (Vector2)transform.position, 1 - slowDownDamping);
            transform.position = Vector2.Lerp(transform.position, target, time);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(target, 0.1f);
    }
}
