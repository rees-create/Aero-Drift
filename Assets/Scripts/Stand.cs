using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class Stand : MonoBehaviour
{
    public bool active;
    public bool recordingStance;

    public UnwrapObject beforeStance;
    public UnwrapObject stance;
    public List<UnwrapObject> afterStances;


    [System.Serializable]
    public class TransformKVP {
        public GameObject key;
        public Vector3 value;
        public TransformKVP(GameObject key, Vector3 value)
        {
            this.key = key;
            this.value = value;
        }

        public static bool KVPListContains(List<TransformKVP> list, GameObject key)
        {
            foreach (TransformKVP kvp in list)
            {
                if (kvp.key.Equals(key))
                {
                    return true;
                }
            }
            return false;
        }
        public static void KVPListModify(ref List<TransformKVP> list, GameObject key, TransformKVP newValue)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].key.Equals(key))
                {
                    list[i] = newValue;
                    break;
                }
            }
        }
        public static TransformKVP KVPListGet(List<TransformKVP> list, GameObject newKey)
        {
            print($"List count: {list.Count}");
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].key.Equals(newKey))
                {
                    return list[i];
                }
            }
            return null;
        }
    }
    [System.Serializable]
    public class UnwrapObject
    {
        public bool setStance;
        //Recursively scan through all child objects for position, rotation and scale
        [SerializeField] public List<TransformKVP> position = new List<TransformKVP>();
        public List<TransformKVP> rotation = new List<TransformKVP>();
        public List<TransformKVP> scale = new List<TransformKVP>();
        

        public UnwrapObject interpolatedUnwrap;

        bool KVPListContains<T, U>(List<KeyValuePair<T, U>> list, T key)
        {
            foreach (KeyValuePair<T, U> kvp in list)
            {
                if (kvp.Key.Equals(key))
                {
                    return true;
                }
            }
            return false;
        }
        public void KVPListModify<T, U>(ref List<KeyValuePair<T, U>> list, T key, KeyValuePair<T, U> newValue)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Key.Equals(key))
                {
                    list[i] = newValue;
                    break;
                }
            }
        }
        public KeyValuePair<T, U> KVPListGet<T, U>(List<KeyValuePair<T, U>> list, T key) 
        {
            print($"List count: {list.Count}");
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Key.Equals(key))
                {
                    return list[i];
                }
            }
            return new KeyValuePair<T, U>();
        }

        public void SetTransforms(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                if (!TransformKVP.KVPListContains(position, parent.GetChild(i).gameObject))
                {
                    position.Add(new TransformKVP(parent.GetChild(i).gameObject, parent.GetChild(i).localPosition));
                }
                if (!TransformKVP.KVPListContains(rotation, parent.GetChild(i).gameObject))
                { 
                    rotation.Add(new TransformKVP(parent.GetChild(i).gameObject, parent.GetChild(i).localEulerAngles));
                }
                if (!TransformKVP.KVPListContains(scale, parent.GetChild(i).gameObject))
                {
                    scale.Add(new TransformKVP(parent.GetChild(i).gameObject, parent.GetChild(i).localScale));
                }
                // Recursively scan children
                if (parent.GetChild(i).childCount > 0)
                {
                    SetTransforms(parent.GetChild(i));
                }
            }
            
            //modified
        }
        //lerp betweeen this and another dictionary set
        public void ThisToThat(Transform parent, UnwrapObject that, float t)
        {
            //Initialize interpolated unwrap
            interpolatedUnwrap = new UnwrapObject();
            interpolatedUnwrap.SetTransforms(parent);
            //scan the object
            //test code
            string list = "";
            foreach (TransformKVP pair in position)
            {
                list += pair.key.name + ", ";
            }
            string that_list = "";
            foreach (TransformKVP pair in that.position)
            {
                that_list += pair.key.name + ", ";
            }
            print($"{parent.gameObject.name} position dict keys: {list}, that position dict keys: {that_list}");
            //end test code
            foreach (Transform child in parent)
            {
                Vector3 localPosition = TransformKVP.KVPListGet(this.position, child.gameObject).value;
                Vector3 thatPosition = TransformKVP.KVPListGet(that.position, child.gameObject).value;
                TransformKVP positionPair = new TransformKVP(child.gameObject, Vector3.Lerp(localPosition, thatPosition, t)); 
                TransformKVP.KVPListModify(ref interpolatedUnwrap.position, child.gameObject, positionPair);

                Vector3 localRotation = TransformKVP.KVPListGet(this.rotation, child.gameObject).value;
                Vector3 thatRotation = TransformKVP.KVPListGet(that.rotation, child.gameObject).value;
                TransformKVP rotationPair = new TransformKVP(child.gameObject, Vector3.Lerp(localRotation, thatRotation, t));
                TransformKVP.KVPListModify(ref interpolatedUnwrap.rotation, child.gameObject, rotationPair); 

                Vector3 localScale = TransformKVP.KVPListGet(this.rotation, child.gameObject).value;
                Vector3 thatScale = TransformKVP.KVPListGet(that.rotation, child.gameObject).value;
                TransformKVP scalePair = new TransformKVP(child.gameObject, Vector3.Lerp(localScale, thatScale, t));
                TransformKVP.KVPListModify(ref interpolatedUnwrap.scale, child.gameObject, scalePair);
                // Recursively scan children
                if (child.childCount > 0)
                    ThisToThat(child, that, t);
            }
        }

    }
    
    IEnumerator SetStances()
    {
        while (recordingStance) 
        {
            if (beforeStance.setStance)
            {
                print("before stance set");
                beforeStance.SetTransforms(gameObject.transform);
            }
            if (stance.setStance)
            {
                print("after stance set");
                stance.SetTransforms(gameObject.transform);
            }
            for (int i = 0; i < afterStances.Count; i++)
            {
                if (afterStances[i].setStance)
                {
                    afterStances[i].SetTransforms(gameObject.transform);
                }
            }
            yield return new WaitForSeconds(0.1f); // wanted a slower update time that's why i'm even using a coroutine.
        }
        StopCoroutine(SetStances());
    }
    IEnumerator PlayStanceTransition(UnwrapObject stance1, UnwrapObject stance2) {
        float animTime = 0f;
        while (animTime <= 1)
        {
            stance1.ThisToThat(gameObject.transform, stance2, animTime);
            animTime += (Time.deltaTime / 1f); //1 second transition time
            yield return new WaitForEndOfFrame();
        }
    }
    public enum TransitionState { In, Out };
    IEnumerator StandLoop(TransitionState state) 
    {
        while (true)
        {
            if (recordingStance)
            {
                print("recording stance");
                StartCoroutine(SetStances());

                //test code
                string list = "";
                foreach (TransformKVP pair in beforeStance.position)
                {
                    list += pair.key.name + ", ";
                }
                string that_list = "";
                foreach (TransformKVP pair in stance.position)
                {
                    that_list += pair.key.name + ", ";
                }
                print($"position dict keys: {list}, that position dict keys: {that_list}");
                //end test code

            }
            yield return new WaitUntil(()=> active == true);
            if (state == TransitionState.In)
            {
                StartCoroutine(PlayStanceTransition(beforeStance, stance));
            }
            else if (state == TransitionState.Out)
            {
                //do afterStance[0] for now, IX control will handle this later.
                StartCoroutine(PlayStanceTransition(stance, afterStances[0]));
            }
            active = false;
        }
    }

    // Start is called before the first frame update
    
    void Start()
    {
        StartCoroutine(StandLoop(TransitionState.In));
    }

    // Update is called once per frame
    void Update()
    {
        
    }





}
