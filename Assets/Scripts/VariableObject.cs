using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class VariableObject : MonoBehaviour
{
    [SerializeField] List<GameObject> objects;
    
    bool unparentAndSelfDestruct;
    [SerializeField] float decision;
    // Start is called before the first frame update

    
    void Start()
    {
        StartCoroutine(PickObject());
    }

    // Update is called once per frame
    IEnumerator PickObject()
    {
        //choose object to instantiate
        ObjectSpawnSystem objectSpawnSystem = gameObject.GetComponentInParent<ObjectSpawnSystem>();
        if (objectSpawnSystem != null)
        {
            decision = objectSpawnSystem.elementVariation.rand(objectSpawnSystem.elementVariation.phaseSeed, gameObject.transform.GetSiblingIndex(), 0);
        }
        else
        {
            decision = Random.Range(0f, 1f);
        }
        int objectIndex = (int) (decision * objects.Count);
        //Instantiate object
        Instantiate(objects[objectIndex], transform);

        float oldDecision = decision;
        yield return new WaitUntil(()=> oldDecision != decision);
    }
}
