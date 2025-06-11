using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

//[ExecuteInEditMode]
public class EscalatorAnim : MonoBehaviour
{
    [SerializeField] Vector2 path;
    [SerializeField] int numberOfObjects;
    [SerializeField] float squishFactor;
    [SerializeField] GameObject stair;
    [SerializeField] float speed;
    [SerializeField] bool playAnimation;
    List<GameObject> stairs;
    List<FrameCounter> frameCounters;

    class FrameCounter 
    {
        float count;
        int index;
        public FrameCounter(int index) 
        {
            this.index = index;
        }
        public float Count
        {
            get { return count; }
            set { count = value; }
        }
        public void Increment(float frameCount, float speed)
        {
            count = (count + speed) % frameCount;
        }
        public float CalculateFrameCount(float frames, int numberOfObjects, float squishFactor)
        {
            return count + index * (int) (frames / (numberOfObjects * squishFactor));
        }
        public void Reset()
        {
            count = 0;
        }

    }
    IEnumerator StairSpawnLoop()
    {
        while (true)
        {
            for (int i = 0; i < numberOfObjects; i++)
            {
                GameObject newStair = Instantiate(stair);
                newStair.transform.parent = transform;
                newStair.name = $"Stair {i + 1}"; // Name the stair for easier identification in the hierarchy
                float stairPosition = (float)i / numberOfObjects; // position of stair in the list
                newStair.transform.localPosition = new Vector2(stairPosition * path.x, stairPosition * path.y);
                
                stairs.Add(newStair);
            }

            int oldNumberOfObjects = numberOfObjects;
            yield return new WaitUntil(() => numberOfObjects != oldNumberOfObjects); // Delay before next cycle
            //TODO: if going to next cycle destroy clones then recreate.
        }
    }
    IEnumerator PrintFrameCounter() 
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Print every second
            for (int i = 0; i < frameCounters.Count; i++)
            {
                Debug.Log($"Stair {i + 1} Frame Counter: {frameCounters[i]}");
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        stairs = new List<GameObject>();
        frameCounters = new List<FrameCounter>();
        for (int i = 0; i < numberOfObjects; i++)
        {
            frameCounters.Add(new FrameCounter(i)); // Initialize frame counters for each stair
        }
        StartCoroutine(StairSpawnLoop());
        //StartCoroutine(PrintFrameCounter());
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // Move each stair to end of path vector then pop back to beginning
        
        if (playAnimation)
        {
            float FPS = 1 / Time.deltaTime;
            float frames = FPS / speed; // Calculate frames based on speed and FPS
            for (int i = 0; i < numberOfObjects; i++) 
            {
                float currentStairFrameCount = frameCounters[i].CalculateFrameCount(frames, numberOfObjects, squishFactor) % frames;
                float cycleProgress = currentStairFrameCount / frames; // currentStairFrameCount normalized
                Vector2 cycleValue = Vector2.Lerp(Vector2.zero, path, cycleProgress);
                stairs[i].transform.localPosition = cycleValue;
                frameCounters[i].Increment(frames, speed);
            }
            
        }
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, path);
    }
}
