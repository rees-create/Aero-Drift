using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    List<FrameCounter> frameCounter;

    class FrameCounter 
    {
        int count;
        int index;
        public FrameCounter(int index) 
        {
            this.index = index;
        }
        public int Count
        {
            get { return count; }
            set { count = value; }
        }
        public void Increment()
        {
            count++;
        }
        public int CalculateFrameCount(float frames, int numberOfObjects, float squishFactor)
        {
            return count + index * (int)((frames / numberOfObjects) / squishFactor);
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
            for (int i = 0; i < frameCounter.Count; i++)
            {
                Debug.Log($"Stair {i + 1} Frame Counter: {frameCounter[i]}");
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        stairs = new List<GameObject>();
        frameCounter = new List<FrameCounter>();
        for (int i = 0; i < numberOfObjects; i++)
        {
            frameCounter.Add(new FrameCounter(i)); // Initialize frame counters for each stair
        }
        StartCoroutine(StairSpawnLoop());
        //StartCoroutine(PrintFrameCounter());
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // Move each stair to end of path vector then pop back to beginning
        //print("hello?");
        if (playAnimation)
        { 
            float FPS = 1 / Time.deltaTime;
            float frames = FPS / speed;
            
            //bool resetFrameCounter = false;
            for (int i = 0; i < stairs.Count; i++)
            {
                //frameCounter[i] = i * (int) ((frames / numberOfObjects) / squishFactor);
                float fullCycleProgress = frameCounter[i].CalculateFrameCount(frames, numberOfObjects, squishFactor)/frames;
                
                float stairPosFraction = (float)i / (float)stairs.Count;
                //Vector2 stairInitPosition = new Vector2(stairPosFraction * path.x, stairPosFraction * path.y);
                float localAnimProgress = stairPosFraction * (1/frames) + fullCycleProgress; // if this is complete pop back stair... there's still more to do
                print($"Stair {i+1}: fullCycleProgress = {fullCycleProgress}");
                if (fullCycleProgress >= 1f)
                {
                    print("exceeded 1");
                    //swap stair to beginning of list
                    for (int j = stairs.Count; j < 1; j--)
                    {
                        GameObject temp = stairs[j];
                        stairs[j] = stairs[j - 1];
                        stairs[j - 1] = temp;
                        DestroyImmediate(temp); //don't litter the hierarchy
                    }
                    stairs[0].transform.localPosition = Vector2.zero; // reset stair to start position
                    //resetFrameCounter = true; // reset full cycle progress
                    frameCounter[i].Reset(); // reset frame counter for this stair
                }
                Vector2 updatedPosition = Vector2.Lerp(Vector2.zero, path, fullCycleProgress);
                //print($"localAnimProgress = {localAnimProgress}; stair {i} height = {stairPosFraction * path}");
                stairs[i].transform.localPosition = stairPosFraction * path + ((updatedPosition) / (Vector2)stairs[i].transform.localScale);
                
                //move frame counter forward
                frameCounter[i].Increment();
            }
            
            // Reset frame counter if it exceeds the number of frames
            //if (resetFrameCounter)
            //{
            //    frameCounter[] = 0;
            //}
        }
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, path);
    }
}
