using System;
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
    [Header("(Temp) Script Patches as Options")]
    public bool useScaleForTriggerPoint;
    public bool useV2Coroutine;
    [Header("Update Loop")]
    public bool useUpdateFunction;
    IEnumerator PopBackStartBeforeEnd() {
        while (true)
        {
            while (true)
            {
                startBlock.GetComponent<ObjectSpawnSystem>().popBack = false;
                ObjectSpawnSystem mainSystem = mainBlock.GetComponent<ObjectSpawnSystem>();
                int nObjects = mainSystem.elementVariation.numberOfObjects;
                int popBackAtLast = mainSystem.elementVariation.popBackAtLast;
                float popBackProximity = mainSystem.elementVariation.popBackProximity;
                float spawnSpacingX = mainSystem.elementVariation.spawnSpacing.x;
                float scaleX = mainSystem.elementVariation.scale.x;
                //calculate pop back x position;
                float popBackX = !useScaleForTriggerPoint ? 
                    (nObjects - popBackAtLast) * spawnSpacingX + popBackProximity :
                    (nObjects - popBackAtLast) * spawnSpacingX * scaleX + popBackProximity;
                Vector2 triggerPoint = (mainBlock.transform.position + new Vector3(popBackX, 0, 0)) - Vector3.right * triggerProximity;
                //wait until player reaches trigger point
                yield return new WaitUntil(() => player.transform.position.x >= triggerPoint.x);
                break;
            }
            while (!mainBlock.GetComponent<ObjectSpawnSystem>().popBack)
            {
                bool begin1PhaseSeedBehind = Mathf.Abs(endBlock.elementVariation.phaseSeed - startBlock.elementVariation.phaseSeed - seedJump) < MANTISSA_WINDOW;
                //bool beginBehind = endBlock.elementVariation.phaseSeed >= startBlock.elementVariation.phaseSeed;
                if (begin1PhaseSeedBehind)
                {
                    startBlock.GetComponent<ObjectSpawnSystem>().elementVariation.phaseSeed += seedJump;
                }
                Debug.Log("pop back triggered");
                startBlock.GetComponent<ObjectSpawnSystem>().popBack = true;
                yield return new WaitUntil(() => mainBlock.GetComponent<ObjectSpawnSystem>().popBack);
            }
        }
    }
    IEnumerator PopBackStartBeforeEndV2()
    {
        while (true)
        {
            
            startBlock.GetComponent<ObjectSpawnSystem>().popBack = false;
            ObjectSpawnSystem mainSystem = mainBlock.GetComponent<ObjectSpawnSystem>();
            int nObjects = mainSystem.elementVariation.numberOfObjects;
            int popBackAtLast = mainSystem.elementVariation.popBackAtLast;
            float popBackProximity = mainSystem.elementVariation.popBackProximity;
            float spawnSpacingX = mainSystem.elementVariation.spawnSpacing.x;
            float scaleX = mainSystem.elementVariation.scale.x;
            //calculate pop back x position;
            float popBackX = !useScaleForTriggerPoint ?
                (nObjects - popBackAtLast) * spawnSpacingX + popBackProximity :
                (nObjects - popBackAtLast) * spawnSpacingX * scaleX + popBackProximity;
            Vector2 triggerPoint = (mainBlock.transform.position + new Vector3(popBackX, 0, 0)) - Vector3.right * triggerProximity;
            //wait until player reaches trigger point
            yield return new WaitUntil(() => player.transform.position.x >= triggerPoint.x);
                
            while (!mainBlock.GetComponent<ObjectSpawnSystem>().popBack)
            {
                bool begin1PhaseSeedBehind = Mathf.Abs(endBlock.elementVariation.phaseSeed - startBlock.elementVariation.phaseSeed - seedJump) < MANTISSA_WINDOW;
                //bool beginBehind = endBlock.elementVariation.phaseSeed >= startBlock.elementVariation.phaseSeed;
                if (begin1PhaseSeedBehind)
                {
                    startBlock.GetComponent<ObjectSpawnSystem>().elementVariation.phaseSeed += seedJump;
                }
                Debug.Log("pop back triggered");
                startBlock.GetComponent<ObjectSpawnSystem>().popBack = true;
                yield return new WaitUntil(() => mainBlock.GetComponent<ObjectSpawnSystem>().popBack);
            }
        }
    }

    int updateSyncSwitch;
    void UpdatePopBackSync() 
    {
        startBlock.GetComponent<ObjectSpawnSystem>().popBack = false;
        ObjectSpawnSystem mainSystem = mainBlock.GetComponent<ObjectSpawnSystem>();
        int nObjects = mainSystem.elementVariation.numberOfObjects;
        int popBackAtLast = mainSystem.elementVariation.popBackAtLast;
        float popBackProximity = mainSystem.elementVariation.popBackProximity;
        float spawnSpacingX = mainSystem.elementVariation.spawnSpacing.x;
        float scaleX = mainSystem.elementVariation.scale.x;
        //calculate pop back x position;
        float popBackX = !useScaleForTriggerPoint ?
            (nObjects - popBackAtLast) * spawnSpacingX + popBackProximity :
            (nObjects - popBackAtLast) * spawnSpacingX * scaleX + popBackProximity;
        Vector2 triggerPoint = (mainBlock.transform.position + new Vector3(popBackX, 0, 0)) - Vector3.right * triggerProximity;

        if (player.transform.position.x >= triggerPoint.x && updateSyncSwitch <= 0)
        {
            updateSyncSwitch++;
            bool begin1PhaseSeedBehind = Mathf.Abs(endBlock.elementVariation.phaseSeed - startBlock.elementVariation.phaseSeed - seedJump) < MANTISSA_WINDOW;
            //bool beginBehind = endBlock.elementVariation.phaseSeed >= startBlock.elementVariation.phaseSeed;
            if (begin1PhaseSeedBehind)
            {
                startBlock.GetComponent<ObjectSpawnSystem>().elementVariation.phaseSeed += seedJump;
            }
            Debug.Log("pop back triggered");
            startBlock.GetComponent<ObjectSpawnSystem>().popBack = true;
            
        }
        else 
        {
            updateSyncSwitch = 0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!useV2Coroutine)
        {
            StartCoroutine(PopBackStartBeforeEnd());
        }
        else 
        {
            StartCoroutine(PopBackStartBeforeEndV2());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (useUpdateFunction) { 
            UpdatePopBackSync();
        }
    }
}
