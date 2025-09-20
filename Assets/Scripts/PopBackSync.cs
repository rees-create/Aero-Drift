using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopBackSync : MonoBehaviour
{
    const float MANTISSA_WINDOW = 0.00001f;

    public ObjectSpawnSystem startBlock;
    public ObjectSpawnSystem endBlock;
    public GameObject mainBlock;
    public GameObject player;
    public float triggerProximity;
    public float frequency = 1f;
    public float seedJump = 0f;
    
    IEnumerator PopBackStartBeforeEnd() {
        //collect needed values
        while (true)
        {
            ObjectSpawnSystem mainSystem = mainBlock.GetComponent<ObjectSpawnSystem>();
            int nObjects = mainSystem.elementVariation.numberOfObjects;
            int popBackAtLast = mainSystem.elementVariation.popBackAtLast;
            float popBackProximity = mainSystem.elementVariation.popBackProximity;
            float spawnSpacingX = mainSystem.elementVariation.spawnSpacing.x;
            //calculate pop back x position;
            float popBackX = (nObjects - popBackAtLast) * spawnSpacingX + popBackProximity;
            Vector2 triggerPoint = (mainBlock.transform.position + new Vector3(popBackX, 0, 0)) - Vector3.right * triggerProximity;
            //wait until player reaches trigger point
            yield return new WaitUntil(() => player.transform.position.x >= triggerPoint.x);

            bool begin1PhaseSeedBehind = Mathf.Abs(endBlock.elementVariation.phaseSeed - startBlock.elementVariation.phaseSeed - seedJump) < MANTISSA_WINDOW;
            if (begin1PhaseSeedBehind)
            {
                startBlock.GetComponent<ObjectSpawnSystem>().elementVariation.phaseSeed += seedJump;
            }
            startBlock.GetComponent<ObjectSpawnSystem>().popBack = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PopBackStartBeforeEnd());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
