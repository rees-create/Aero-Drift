using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

public class Collectible : MonoBehaviour
{
    //just trigger the animation once collided with by a player, wait till the end, then destroy the object
    // Start is called before the first frame update
    //TODO: Sync your recent commit when you get home.
    public string triggerName;
    public string animationName;
    [System.NonSerialized] GameObject plane;
    [SerializeField] float followSpeed;

    Animator animator;
    float animationTime;
    float[] animationTimes = new float[] { 0, 0 };
    float[] animTimeDiffs = new float[] { 0, 0 };
    float duration;
    bool animationReady = false;
    int diffSwitch = 0;
    bool follow = false;
    int sameSignCounter = 0;
    
    Vector3 oldBaitPosition;

    GameObject GetObjectSpawnSystem(GameObject start)
    {
        Transform tr = start.transform.parent;
        if (tr.GetComponent<ObjectSpawnSystem>() == null)
        {
            return GetObjectSpawnSystem(tr.gameObject);
        }
        else
        {
            return tr.gameObject;
        }
    }

    void Follow(float followSpeed)
    {
        //print($"following: transform.position = {transform.position}");
        Vector3 newBaitPosition = plane.transform.position;
        if (oldBaitPosition != default)
        {
            transform.position += (newBaitPosition - oldBaitPosition) * followSpeed;
        }
        oldBaitPosition = newBaitPosition;

    }
    IEnumerator WaitForCollect() 
    {
        if (GetComponent<BoxCollider2D>() != null) 
        {
            gameObject.GetComponent<Animator>().SetTrigger(triggerName);
            //print($"GetTrigger: {gameObject.GetComponent<Animator>().GetBool(triggerName)}");
            follow = true;
            yield return new WaitUntil(()=> animationReady && animationTime >= 1);
            follow = false;
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        //print("hello?");
        animator = GetComponent<Animator>();
        plane = GetObjectSpawnSystem(gameObject).GetComponent<CollectibleHandler>().plane;
        //print(plane.name);
        //find animation duration
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip cl = clips[i];
            if (cl.name == animationName)
            {
                duration = cl.length;
                break;
            }
        }
    }

    IEnumerator OnTriggerEnter2D(Collider2D other)
    {
        //print($"other gameobject name: {other.gameObject.name} plane gameObject name= {plane.name}");
        //if (Mathf.Abs(animationTimes[0] - animationTimes[1]) * duration > 2.5 * Time.deltaTime)
        //{

        //}
        if (other.name == plane.name)
        {
            //yield return new WaitUntil(() => animationReady);
            StartCoroutine(WaitForCollect());
        }
        yield return null;
    }
    bool SameSign(float a, float b) 
    {
        bool bothPositiveOrNegative = (a >= 0) == (b >= 0);
        bool containsZero = (a == 0) || (b == 0);
        bool notAllZero = !((a == 0) && (b == 0));

        return (bothPositiveOrNegative || containsZero) && notAllZero;
    }
    // Update is called once per frame
    void Update()
    {
        diffSwitch = (diffSwitch + 1) % 2;
        animationTimes[diffSwitch] = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        animationTime = animationTimes[diffSwitch];
        animTimeDiffs[diffSwitch] = animationTimes[0] - animationTimes[1];
        if(animationTime < 0.5) 
        {
            //print($"starting animTimeDiffs: {animTimeDiffs[0]}, {animTimeDiffs[1]} sameSignCounter = {sameSignCounter}");
        }

        if (SameSign(animTimeDiffs[0], animTimeDiffs[1]))
        {
            sameSignCounter++;
            //print($"animTimeDiffs: {animTimeDiffs[0]}, {animTimeDiffs[1]} sameSignCounter = {sameSignCounter}");
        }
        else 
        {
            sameSignCounter = 0;
        }
        if (sameSignCounter >= 2) 
        {
            animationReady = true;
        }
        //else if (animationTimes[1] == 0 || animationTimes[0] == 0) 
        //{
            //print($"did it reset? animationTimes: {animationTimes[0]}, {animationTimes[1]}");
        //}
        
        if (follow) 
        {
            Follow(followSpeed);
        }
    }
}
