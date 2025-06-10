using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling;
using UnityEditor.Rendering;
using UnityEditor.Tilemaps;
using UnityEngine;

//[ExecuteInEditMode]
public class EscalatorAnim : MonoBehaviour
{
    [SerializeField] Vector2 path;
    [SerializeField] int numberOfObjects;
    [SerializeField] GameObject stair;
    [SerializeField] float speed;
    [SerializeField] bool playAnimation;
    List<GameObject> stairs;
    int frameCounter = 0;
    
    IEnumerator StairSpawnLoop()
    {
        while (true)
        {
            for (int i = 0; i < numberOfObjects; i++)
            {
                GameObject newStair = Instantiate(stair);
                newStair.transform.parent = transform;
                float stairPosition = (float)i / numberOfObjects; // position of stair in the list
                newStair.transform.localPosition = new Vector2(stairPosition * path.x, stairPosition * path.y);
                
                stairs.Add(newStair);
                print($"{newStair.transform.localPosition}");
            }

            int oldNumberOfObjects = numberOfObjects;
            yield return new WaitUntil(() => numberOfObjects != oldNumberOfObjects); // Delay before next cycle
            //TODO: if going to next cycle destroy clones then recreate.
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        stairs = new List<GameObject>();
        StartCoroutine(StairSpawnLoop());
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
        float fullCycleProgress = frameCounter / frames;
        bool resetFrameCounter = false;
        for (int i = 0; i < stairs.Count; i++)
        {
            float stairPosFraction = (float)i / (float)stairs.Count;
            //Vector2 stairInitPosition = new Vector2(stairPosFraction * path.x, stairPosFraction * path.y);
            float localAnimProgress = stairPosFraction * frames + fullCycleProgress; // if this is complete pop back stair... there's still more to do
            if (localAnimProgress >= 1)
            {
                //swap stair to beginning of list
                for (int j = stairs.Count; j < 1; j--)
                {
                    GameObject temp = stairs[j - 1];
                    stairs[j] = stairs[j - 1];
                    stairs[j - 1] = temp;
                    DestroyImmediate(temp); //don't litter the hierarchy
                }
                stairs[0].transform.localPosition = Vector2.zero; // reset stair to start position
                resetFrameCounter = true; // reset full cycle progress
            }
            Vector2 updatedPosition = Vector2.Lerp(Vector2.zero, path, localAnimProgress);
            print($"updated position = {updatedPosition}; stair {i} height = {stairPosFraction * path}");
            stairs[i].transform.localPosition =  (stairPosFraction * path + updatedPosition) / (Vector2)stairs[i].transform.localScale;
            
        }
        frameCounter++;
        // Reset frame counter if it exceeds the number of frames
        if (resetFrameCounter)
        {
            frameCounter = 0;
        }
    }
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, path);
    }
}
