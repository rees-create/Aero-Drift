using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TerrainTools;
using UnityEngine.UIElements;

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

    const int backlogSize = 5;
    [Range(0, backlogSize)] public int useBacklogIndex;
    [Serializable]
    public struct RandomSpeed 
    {
        public bool active;
        public float minSpeed, maxSpeed;
    }

    //public float popBackStartPoint;

    public float PopBackEndPoint()
    {
        //print($"{gameObject.name} pbtransform: {popBackController.elementVariation.popBackTransform.position}, " +
        //    $"offset: {popBackController.elementVariation.popBackProximity * Vector2.right}");
        return ((Vector2)popBackController.elementVariation.popBackTransform.position -
            (popBackController.elementVariation.popBackProximity * Vector2.right)).x;
    }
    public float DynamicPopBackStartPoint(float popBackEndPoint, bool popBackPointOnly = true)
    {
        float inverseScale = (1f / transform.localScale.x);
        float distToPlaneX = (transform.position.x /** transform.localScale.x*/) - popBackEndPoint;
        if (!popBackPointOnly)
        { 
            distToPlaneX = transform.position.x - plane.transform.position.x;
        }
        print($"{gameObject.transform.parent.name} inverseScale: {inverseScale} distToPlaneX: {distToPlaneX} Position: {transform.position} PopBackEndPoint: {popBackEndPoint}," +
            $"DynamicPopBackStartPoint {((popBackController.GetPlayerInitialPosition().x * inverseScale) + distToPlaneX)}");
        return ((popBackController.GetPlayerInitialPosition().x * inverseScale) + distToPlaneX);// * GetComponent<ObjectSpawnSystem>().elementVariation.scale.x;
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

    float[] popBackEndPointBacklog = new float[backlogSize];
    int backlogIndex = 0;
    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
    //Vector3 markerPos = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        if (plane != null && popBackController != null)
        {
            if (popBackController.PopBackEndPoint() - transform.position.x <= triggerProximity)
            {
                float popBackEndPoint = popBackController.PopBackEndPoint();
                popBackEndPointBacklog[backlogIndex] = popBackEndPoint;

                //print(gameObject.transform.parent.name + " popBackEndPoint" + popBackEndPoint);
                if (popBackController.popBack == true)
                {
                    //print("hello wtf????");
                    if (scaleFromObjectSpawnSystem)
                    {
                        transform.position = new Vector3(DynamicPopBackStartPoint(popBackEndPointBacklog[mod((backlogIndex - useBacklogIndex), popBackEndPointBacklog.Length)]), transform.position.y, transform.position.z);
                    }
                    else
                    {
                        transform.position = new Vector3(DynamicPopBackStartPoint(popBackEndPointBacklog[mod((backlogIndex - useBacklogIndex), popBackEndPointBacklog.Length)]), transform.position.y, transform.position.z); //* transform.localScale.x;
                        //markerPos = transform.position;
                        //Instantiate(gameObject).transform.position = markerPos;
                        //transform.SetPositionAndRotation(markerPos, Quaternion.identity);
                    }
                    //speed = 0;
                }
                
                backlogIndex = (backlogIndex + 1) % backlogSize;
            }
            else 
            {
                //print($"check your ranges: {gameObject.transform.parent.name}- popBackEndPoint = {popBackController.PopBackEndPoint()} x pos = {transform.position.x}");
            }
            
        }
        else 
        {
            Debug.Log("Plane or Pop Back controller is missing");
        }
    }
    private void FixedUpdate()
    {
        if (moveObject)
        {
            Move(speed);
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(markerPos, 1);
        //try
        //{ 

        //}
        //catch (NullReferenceException) 
        //{

        //}
    }
}
