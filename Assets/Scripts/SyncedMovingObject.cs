using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static ObjectSpawnSystem;

public class SyncedMovingObject : MonoBehaviour
{
    public ObjectSpawnSystem popBackController;
    public GameObject plane;
    //public float lengthOffset;
    public float triggerProximity;
    public float speed;
    public bool moveObject;
    public bool scaleFromObjectSpawnSystem;
    public RandomSpeed speedRange;
    [Serializable]
    public struct RandomSpeed 
    {
        public bool active;
        public float minSpeed, maxSpeed;
    }

    //public float popBackStartPoint;

    public float PopBackEndPoint()
    {
        return ((Vector2)popBackController.elementVariation.popBackTransform.position -
            (popBackController.elementVariation.popBackProximity * Vector2.right)).x;
    }
    public float DynamicPopBackStartPoint(bool popBackPointOnly)
    {
        float distToPlaneX = transform.position.x /* (1f/transform.localScale.x)*/ - PopBackEndPoint();
        if (!popBackPointOnly)
        { 
            distToPlaneX = transform.position.x - plane.transform.position.x;
        }
        //print(plane.transform.position.x);
        return (popBackController.GetPlayerInitialPosition().x + distToPlaneX);// * GetComponent<ObjectSpawnSystem>().elementVariation.scale.x;
    }
    public void Move(float speed)
    {
        if (scaleFromObjectSpawnSystem)
        {
            transform.position += GetComponent<ObjectSpawnSystem>().elementVariation.scale.x * Vector3.right * Time.deltaTime * speed;
        }
        else 
        {
            transform.position += transform.localScale.x * Vector3.right * Time.deltaTime * speed;
        }
    }

    public void GetParams(GameObject g) 
    {
        if (!g.GetComponent<SyncedMovingObjectParams>())
        {
            GetParams(g.transform.parent.gameObject);
        }
        else 
        {
            plane = g.GetComponent<SyncedMovingObjectParams>().plane;
            popBackController = g.GetComponent<SyncedMovingObjectParams>().popBackController;
        }
    }
    ObjectSpawnSystem GetUpperObjectSpawnSystem(GameObject g)
    {
        if (!g.GetComponent<ObjectSpawnSystem>())
        {
            return GetUpperObjectSpawnSystem(g.transform.parent.gameObject);
        }
        else
        {
            return g.GetComponent<ObjectSpawnSystem>();
        }
    }
    int GetSpawnSystemIndex(GameObject g)
    {
        string nameEnd = "";
        nameEnd += g.name[^1];
        int index = -1;
        bool hasIndex = int.TryParse(nameEnd, out index);
        if (!hasIndex)
        {
            return GetSpawnSystemIndex(g.transform.parent.gameObject);
        }
        else 
        {
            return index;
        }
    }
    


    // Start is called before the first frame update
    void Start()
    {
        if (popBackController == null || plane == null) 
        {
            GetParams(gameObject);
        }
        if (speedRange.active) 
        {
            ObjectSpawnSystem s = GetUpperObjectSpawnSystem(gameObject);
            float rand = s.elementVariation.rand(s.elementVariation.phaseSeed, GetSpawnSystemIndex(gameObject), 0);
            speed = Mathf.Lerp(speedRange.minSpeed, speedRange.maxSpeed, rand);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (plane != null && popBackController != null)
        {
            if (PopBackEndPoint() - transform.position.x <= triggerProximity)
            {
                if (popBackController.popBack == true)
                {
                    if (scaleFromObjectSpawnSystem)
                    {
                        transform.position = new Vector3(DynamicPopBackStartPoint(true), transform.position.y, transform.position.z);
                    }
                    else
                    {
                        transform.position = new Vector3(DynamicPopBackStartPoint(true), 0, 0) * transform.localScale.x;
                    }
                }
            }
            if (moveObject)
            {
                Move(speed);
            }
        }
        else 
        {
            Debug.Log("Plane or Pop Back controller is missing");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        try
        {
            Gizmos.DrawSphere(new Vector3(PopBackEndPoint(), 0, 0), 1);
        }
        catch (NullReferenceException) 
        {
            
        }
    }
}
