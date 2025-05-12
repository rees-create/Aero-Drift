using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LayerColliderMap;

[ExecuteInEditMode]
public class LayerColliderGroup : MonoBehaviour
{
    public List<LayerCollider> colliders;
    public bool DrawGizmos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnDrawGizmos()
    {
        if (colliders != null && DrawGizmos)
        //draw colliders
        for (int i = 0; i < colliders.Count; i++)
        {
            if (colliders[i] != null)
            {   
                colliders[i].DrawCollider((Vector2)transform.position);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if (colliders != null)
        //    foreach (LayerCollider coll in colliders) 
        //    {
        //        coll.size *= scale; 
        //    }
    }
}
