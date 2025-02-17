using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using static PoseLerp;

[ExecuteInEditMode]
public class PoseLerp : MonoBehaviour
{
    //public List<bool> listenTo = new List<bool>();
    public PoseSequenceManager poseSequenceManager;
    [Range(0, 1)]
    public float lerpValue; // Other scripts should set this value as they see fit e.g Thrower can set 
    //lerpValue = throwIntensity //* lerpValueRange
    //public float lerpValueRange; //change to regular variable and use in update

    //private int currentCount = 0;
    [Serializable]
    public class Pose
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        //[NonSerialized] public GameObject gameObject;
        public bool active = false;
        //[NonSerialized] bool recording;
        public Pose() 
        {
            
        }
        //public bool GetRecording() 
        //{
        //    return recording;
        //}
        //public void SetRecording(bool state) 
        //{
        //    recording = state;
        //}
        IEnumerator ListenForActive(float seconds, GameObject gameObject) 
        {
            while (true) { 
                yield return new WaitUntil(()=> active);
                this.position = gameObject.transform.position;
                this.rotation = gameObject.transform.rotation.eulerAngles;
                this.scale = gameObject.transform.localScale;
                yield return new WaitForSeconds(seconds);
            }
        }
        public Pose(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
        public Pose(GameObject gameObject) 
        {
            this.position = gameObject.transform.position;
            this.rotation = gameObject.transform.rotation.eulerAngles;
            this.scale = gameObject.transform.localScale;
        }
        //Difference is that Record does not make a new GameObject, rather modifies current one.
        public void Record(GameObject gameObject)
        {
            //StartCoroutine(ListenForActive(0.5f));
            this.position = gameObject.transform.localPosition;
            this.rotation = gameObject.transform.localRotation.eulerAngles;
            this.scale = gameObject.transform.localScale;
        }
        public void Assign(ref GameObject gameObject)
        {
            gameObject.transform.localPosition = this.position;
            gameObject.transform.localRotation = Quaternion.Euler(this.rotation);
            gameObject.transform.localScale = this.scale;
            //print($"GameObject {gameObject.name} assigned; new rotation = {gameObject.transform.localRotation.eulerAngles}");
        }

    }

    public Pose LerpPose(Pose pose1, Pose pose2, float t)
    {
        Pose resultPose = new Pose();
        resultPose.position = Vector3.Lerp(pose1.position, pose2.position, t);
        resultPose.scale = Vector3.Lerp(pose1.scale, pose2.scale, t);
        resultPose.rotation = new Vector3(Mathf.LerpAngle(pose1.rotation.x, pose2.rotation.x, t),
                                          Mathf.LerpAngle(pose1.rotation.y, pose2.rotation.y, t),
                                          Mathf.LerpAngle(pose1.rotation.z, pose2.rotation.z, t)
        );
        return resultPose;
    }
    [Serializable]
    public class PoseSequence
    {
        public List<Pose> poses;
        public GameObject gameObject;
        public bool recording;
        [SerializeField] bool setInitPose;
        int setInitPoseAcc = 0;
        public Pose initPose;

        public bool InitPoseOn() 
        { 
            //if (setInitPoseAcc > 0) { setInitPose = false ; }
            return setInitPose; 
        }
        public void SetInitPose() 
        {
            setInitPoseAcc++;
            setInitPose = false;
        }
        
        public PoseSequence() 
        {
            //this.setInitPose = true;
            this.setInitPoseAcc = 0;
            //print(setInitPose); true. Good. If you want to automatically set init pose, add a switch to the pose sequence manager.
            //Debug.Log("created");
        }
        public PoseSequence(List<Pose> poses, GameObject gameObject)
        {
            this.poses = poses;
            this.gameObject = gameObject;

            //this.setInitPose = true;
            this.setInitPoseAcc = 0;
        }
        ~PoseSequence()
        {
            initPose.Assign(ref gameObject); //assign the init pose to the game object on delete;
            Debug.Log("deleted");
        }
    }
    [Serializable]
    public class PoseSequenceManager
    {
        //public int numPoses; // this is for number of animation lerp steps.
        //public int numPoseSequences; // this is for number of game objects that have pose sequences
        public List<PoseSequence> poseSequences = new List<PoseSequence>();
        public bool play;
        public bool resetToInitPose;
        //public void SetInitPose(PoseSequence ps) 
        //{
        //    if (ps.setInitPose)
        //    {
        //        ps.initPose.Record(ps.gameObject);
        //    }
        //    else
        //    {
        //        ps.setInitPose = false; // sorry you can only set initPose once
        //    }
        //}
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (currentCount != listenTo.Count) 
        //{}
       
        
        
        foreach (PoseSequence poseSequence in poseSequenceManager.poseSequences)
        {
            if (poseSequence.InitPoseOn())
            {
                poseSequence.initPose.Record(poseSequence.gameObject);
            }

            if (poseSequenceManager.resetToInitPose) 
            {
                lerpValue = 0;
                poseSequence.initPose.Assign(ref poseSequence.gameObject);
                poseSequenceManager.play = false;
            }
            int notRecording = 0;
            for (int poseIdx = 0; poseIdx < poseSequence.poses.Count; poseIdx++)
            {
                if (poseSequence.poses[poseIdx].active)
                {
                    //record pose
                    poseSequence.recording = true;
                    poseSequence.poses[poseIdx].Record(poseSequence.gameObject);
                }
                else 
                {
                    notRecording++;
                }
                
            }

            if (notRecording == poseSequence.poses.Count) { poseSequence.recording = false; }

            if (poseSequence.InitPoseOn() && poseSequence.recording)
            {
                Debug.Log("Highly unrecommended to record poses when Set Init Pose is on");
                throw new Exception("Highly unrecommended to record poses when Set Init Pose is on");
            }

            if (!(poseSequence.recording || poseSequence.gameObject == null))
            {
                //set current transform property to pose lerp value - uh uh - <pose lerp value> * <num poses> / <lerp value range>
                float poseLerpValue = lerpValue * (poseSequence.poses.Count - 1);
                int lerpIdx = lerpValue <= 1 && lerpValue >= 0 ? (int) Mathf.Floor(poseLerpValue): 0;
                    float t = lerpValue <= 1 && lerpValue >= 0 ? poseLerpValue - lerpIdx : 0;
                if (lerpValue < 1 && poseSequenceManager.play && !poseSequence.InitPoseOn()) // if lerp value not at end of anim
                {
                    Pose pose = LerpPose(poseSequence.poses[lerpIdx], poseSequence.poses[lerpIdx + 1], t);
                    pose.Assign(ref poseSequence.gameObject);
                    
                }
                else if (lerpValue == 1 && poseSequenceManager.play && !poseSequence.InitPoseOn())
                {
                    
                    Pose pose = poseSequence.poses[lerpIdx];
                    pose.Assign(ref poseSequence.gameObject);
                }
                
            }
        }



        // ensure lerp value range is number of poses.
        //if (poseSequenceManager.poseSequences.Count > 0)
        //{
        //    lerpValueRange = poseSequenceManager.poseSequences[0].poses.Count - 1;
        //}
    }
}
