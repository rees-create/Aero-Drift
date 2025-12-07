using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Pusher : MonoBehaviour
{
    public bool active;
    public Animator animator;
    public string animationName;
    public AnimationClip walkAnimation;
    public Vector2 target;
    public float pushSpeed;
    public float slowDownProximity;
    public float slowDownDamping;
    public GameObject popBackController;

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
            float localTime = 0;
            float progress = 0;
            while (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) && progress < 1 && active && walkAnimation == null)
            {
                float time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                progress = Push(time * pushSpeed, initialPosition, target, slowDownProximity, slowDownDamping);
                yield return new WaitForFixedUpdate();
            }
            while (active && progress < 1 && walkAnimation != null && localTime <= 1) 
            {
                localTime += Time.fixedDeltaTime;
                walkAnimation.SampleAnimation(gameObject, localTime);
                Push(localTime * pushSpeed, initialPosition, target, slowDownProximity, slowDownDamping);
                yield return new WaitForFixedUpdate();
            }
            if (Vector2.Distance(transform.position, target) < 0.02f) {
                active = false;
            }
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) || active);
        }
    }
    
    float Push(float time, Vector2 initialPosition, Vector2 target, float slowDownProximityPercent, float slowDownDamping)
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
            Vector2 movingStartPoint = Vector2.Lerp(slowDownPoint, (Vector2)transform.position, 1 - slowDownDamping);
            transform.position = Vector2.Lerp(movingStartPoint, target, time - fullSpeedPercent);
        }
        float progress = Vector2.Distance(initialPosition, transform.position) / Vector2.Distance(initialPosition, target);
        return progress;
    }
    public Vector2 DistanceToTarget(float explorativity, float maxTargetDistance) {
        //target position is chosen as the distance to a random block (out of main blocks/pop back controller)
        Vector2 mainBlockPosition = popBackController.transform.position;
        Vector2 spawnSpacing = popBackController.GetComponent<ObjectSpawnSystem>().elementVariation.spawnSpacing;
        float blockRange = Mathf.Floor(maxTargetDistance * explorativity / spawnSpacing.magnitude);
        float blockOffset = (mainBlockPosition.x - transform.position.x) / spawnSpacing.magnitude;
        Vector2 targetPosition = transform.position;
        //based on block offset set target position
        if (blockOffset >= 0) //positive offset
        {
            //add offset to maximum range
            targetPosition = blockOffset * spawnSpacing + blockRange * spawnSpacing;
        }
        else //negative offset
        {
            //effectiveOffset flips negative fractional offset to put start point at next main block.
            float effectiveOffset = (1 - (Mathf.Abs(blockOffset) % 1));
            targetPosition = effectiveOffset * spawnSpacing + blockRange * spawnSpacing;
        }
        return targetPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(target, 0.8f);
    }
}
