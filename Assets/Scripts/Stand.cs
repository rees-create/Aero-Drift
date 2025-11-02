using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stand : MonoBehaviour
{
    public class UnwrapObject {
        //Recursively scan through all child objects for position, rotation and scale
        public Dictionary<GameObject, Vector3> position = new Dictionary<GameObject, Vector3>();
        public Dictionary<GameObject, Vector3> rotation = new Dictionary<GameObject, Vector3>();
        public Dictionary<GameObject, Vector3> scale = new Dictionary<GameObject, Vector3>();

        public void ScanTransforms(Transform parent)
        {
            //AI-generated code, verify correctness.
            foreach (Transform child in parent)
            {
                position[child.gameObject] = child.localPosition;
                rotation[child.gameObject] = child.localEulerAngles;
                scale[child.gameObject] = child.localScale;
                // Recursively scan children
                if (child.childCount > 0)
                    ScanTransforms(child);
            }
            //end of AI-generated code
        }
        public void LerpToChildTransforms(Transform parent, float t)
        {
            //AI-generated code, verify correctness.
            foreach (Transform child in parent)
            {
                if (position.ContainsKey(child.gameObject))
                {
                    child.localPosition = Vector3.Lerp(child.localPosition, position[child.gameObject], t);
                }
                if (rotation.ContainsKey(child.gameObject))
                {
                    child.localEulerAngles = Vector3.Lerp(child.localEulerAngles, rotation[child.gameObject], t);
                }
                if (scale.ContainsKey(child.gameObject))
                {
                    child.localScale = Vector3.Lerp(child.localScale, scale[child.gameObject], t);
                }
                // Recursively lerp children
                if(child.childCount > 0)
                    LerpToChildTransforms(child, t);
            }
            //end of AI-generated code
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
