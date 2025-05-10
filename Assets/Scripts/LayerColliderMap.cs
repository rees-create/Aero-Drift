using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LayerColliderMap : MonoBehaviour
{
    [SerializeField] GameObject target;
    public List<LayerCollider> colliders;
    [SerializeField] bool useAbsolutePosition;
    public bool clearTarget;
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
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void OnDrawGizmos() 
    {
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
