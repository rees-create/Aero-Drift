using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class LayerColliderMap : MonoBehaviour
{
    [SerializeField] GameObject target;
    public List<LayerCollider> colliders;
    [SerializeField] bool useAbsolutePosition;
    public bool clearTarget;
    public ObjectSpawnSystem popBackController;

    [System.Serializable]
    public class LayerCollider
    {
        public bool on;
        public Vector2 size;
        public Vector2 position;

        public void DrawCollider(Vector2 worldPosition)
        { 
            Gizmos.color = on ? Color.green : Color.red;
            Gizmos.DrawWireCube(worldPosition + position, size);
        }
        public void DrawCollider()
        {
            Gizmos.color = on ? Color.green : Color.red;
            Gizmos.DrawWireCube(position, size);
        }
        //bounding box check
        public bool CheckForTarget(GameObject g, Vector2 worldPosition, bool useAbsolutePosition = true)
        {
            Vector2 v2Pos = new Vector2(g.transform.position.x, g.transform.position.y);
            Vector3 positionDiff = useAbsolutePosition ? v2Pos - position : v2Pos - worldPosition - position;
            if (positionDiff.x < size.x / 2 && positionDiff.x > -size.x / 2 && positionDiff.y < size.y / 2 && positionDiff.y > -size.y / 2)
            {
                return on ? true : false;
            }
            else
            {
                return false;
            }
        }
    }
    IEnumerator ColliderMapResetCycle() 
    {
        while (true)
        {
            yield return new WaitUntil(() => popBackController.popBack || colliders.Count == 0);
            //This loop assumes that the LayerColliderMap is the first child of the parent object.
            for (int i = 1; i < gameObject.transform.parent.childCount; i++)
            {
                SearchForLayerColliders(gameObject.transform.parent.GetChild(i).gameObject);
            }
        }
    }
    public void SearchForLayerColliders(GameObject g, Vector2 scaleMultiplier = default, Vector2 positionAddon = default)
    {
        //TODO: Scale multiplier and position addon
        for (int i = 0; i < g.transform.childCount; i++)
        {
            LayerColliderGroup layerColliderGroup = g.transform.GetChild(i).GetComponent<LayerColliderGroup>();
            //if (g.name.Contains("Apartment")) print("Hello?? Apartment?");
            if (layerColliderGroup != null)
            {
                if (g.name.Contains("Apartment")) print("Hello?? you have a LayerColliderGroup");
                for (int j = 0; j < layerColliderGroup.colliders.Count; j++)
                {
                    if (layerColliderGroup.colliders[j].on)
                    {
                        //print($"Found active LayerCollider on {g.name}");
                        LayerCollider layerCollider = layerColliderGroup.colliders[j];
                        LayerCollider layerColliderCopy = new LayerCollider();
                        positionAddon = layerColliderGroup.gameObject.transform.position;
                        layerCollider.position += (Vector2) positionAddon;
                        layerCollider.size *= scaleMultiplier;
                        colliders.Add(layerColliderGroup.colliders[j]);
                        print("Hello??");
                    }
                }
            }
            else 
            {
                if (g.transform.GetComponent<ObjectSpawnSystem>())
                {
                    scaleMultiplier = (Vector2) g.transform.GetComponent<ObjectSpawnSystem>().elementVariation.scale;
                    //print($"ObjectSpawnSystem for {g.name} has scale {scaleMultiplier}");
                }
                if (g.transform.childCount > 0)
                {
                    //print($"Next step in tree: {g.transform.GetChild(i).gameObject.name}");
                    SearchForLayerColliders(g.transform.GetChild(i).gameObject, scaleMultiplier, positionAddon);
                }
                else
                {
                    print($"dump recursion here {g.name} transform.childCount = {transform.childCount}");
                }
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //This loop assumes that the LayerColliderMap is the first child of the parent object.
        //for (int i = 1; i < gameObject.transform.parent.childCount; i++)
        //{
        //    SearchForLayerColliders(gameObject.transform.parent.GetChild(i).gameObject);
        //}
        StartCoroutine(ColliderMapResetCycle());
    }
    void OnDrawGizmos() 
    {
        if (colliders != null)
            //draw colliders
            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i] != null)
                {
                    if (useAbsolutePosition)
                    {
                        colliders[i].DrawCollider();
                    }
                    else
                    {
                        colliders[i].DrawCollider((Vector2) transform.position);
                    }       
                }
            }   
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (target != null)
        {
            int clearCount = 0;
            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i].CheckForTarget(target, transform.position, useAbsolutePosition))
                {
                    clearTarget = false;
                    break;
                }
                else
                {
                    //colliders[i].hit = false;
                    clearCount++;
                }
            }
            if(clearCount == colliders.Count)
            {
                clearTarget = true;
            }
        }
    }
    
}
