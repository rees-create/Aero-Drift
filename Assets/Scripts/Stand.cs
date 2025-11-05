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
    [System.Serializable]
    public class UnwrapObject
    {
        public bool setStance;
        //Recursively scan through all child objects for position, rotation and scale
        public Dictionary<GameObject, Vector3> position = new Dictionary<GameObject, Vector3>();
        public Dictionary<GameObject, Vector3> rotation = new Dictionary<GameObject, Vector3>();
        public Dictionary<GameObject, Vector3> scale = new Dictionary<GameObject, Vector3>();
        

        public UnwrapObject interpolatedUnwrap;

        public void SetTransforms(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                if (!position.ContainsKey(parent.GetChild(i).gameObject))
                {
                    position.Add(parent.GetChild(i).gameObject, parent.GetChild(i).localPosition);
                }
                if (!rotation.ContainsKey(parent.GetChild(i).gameObject))
                { 
                    rotation.Add(parent.GetChild(i).gameObject, parent.GetChild(i).localEulerAngles);
                }
                if (!scale.ContainsKey(parent.GetChild(i).gameObject))
                {
                    scale.Add(parent.GetChild(i).gameObject, parent.GetChild(i).localScale);
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
            foreach (GameObject g in position.Keys)
            {
                list += g.name + ", ";
            }
            string that_list = "";
            foreach (GameObject g in that.position.Keys)
            {
                list += g.name + ", ";
            }
            print($"{parent.gameObject.name} position dict keys: {list}, that position dict keys: {that_list}");
            //end test code
            foreach (Transform child in parent)
            {
                interpolatedUnwrap.position[child.gameObject] = Vector3.Lerp(this.position[child.gameObject], that.position[child.gameObject], t);
                interpolatedUnwrap.rotation[child.gameObject] = Vector3.Lerp(this.rotation[child.gameObject], that.rotation[child.gameObject], t);
                interpolatedUnwrap.scale[child.gameObject] = Vector3.Lerp(this.scale[child.gameObject], that.scale[child.gameObject], t);
                // Recursively scan children
                if (child.childCount > 0)
                    ThisToThat(child, that, t);
            }
        }

    }
    public UnwrapObject beforeStance;
    public UnwrapObject stance;
    public List<UnwrapObject> afterStances;
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
                foreach (GameObject g in beforeStance.position.Keys)
                {
                    list += g.name + ", ";
                }
                string that_list = "";
                foreach (GameObject g in beforeStance.position.Keys)
                {
                    list += g.name + ", ";
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
