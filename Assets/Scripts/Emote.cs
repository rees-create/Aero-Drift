using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Emote : MonoBehaviour
{
    public bool active;
    public List<AnimationClip> emotes;
    public List<NPCPersonality> emotePersonalities;
    public float speed;
    NPCPersonality hostPersonality;

    [System.Serializable]
    public struct NPCPersonality
    {
        [SerializeField] [Range(0, 1)] float planeAffinity;
        public float PlaneAffinity 
        {
            get {
                return planeAffinity;
            }
            set {
                planeAffinity = value;
            }
        }
        [SerializeField] [Range(0, 1)] float explorativity;
        public float Explorativity
        {
            get 
            {
                return explorativity;
            }
            set 
            {
                explorativity = value;
            }
        }
        [System.NonSerialized] public float meanSelectionScore;
        public void SetPlaneAffinity(float value) 
        {
            planeAffinity = value;
        }
        public void SetExplorativity(float value) 
        {
            explorativity = value;
        }
        public void SetMeanSelectionScore(float value) 
        {
            meanSelectionScore = value;
        }
    }

    public void SetActive(float planeAffinity, float explorativity)
    {
        active = true;
        hostPersonality.SetExplorativity(explorativity);
        hostPersonality.SetPlaneAffinity(planeAffinity);
        //for (int i = 0; i < emotePersonalities.Count; i++)
        //{
        //    if (personalityIndex == i)
        //    {
        //        emoteIndex = i;
        //    }
        //}
         // do this in special function
    }

    IEnumerator EmoteSequence() 
    {
        while (true)
        {
            yield return new WaitUntil(()=> active);
            //print("playing emote");
            StartCoroutine(PlayAnimation());
            active = false;
        }
    }

    IEnumerator PlayAnimation() 
    {
        int emoteIndex = SelectAnimation(hostPersonality.PlaneAffinity, hostPersonality.Explorativity);
        AnimationClip emote = emotes[emoteIndex];
        //print($"{gameObject.name}: {emote.name} emote");
        float time = 0;
        while (time < 1)
        {
            emote.SampleAnimation(gameObject, time);
            time += speed * Time.deltaTime;
            //print($"{gameObject.name} emote anim time = {time}");
            yield return new WaitForEndOfFrame();
        }
    }
    public int SelectAnimation(float planeAffinity, float explorativity)
    {
        List<NPCPersonality> personalityProducts = new List<NPCPersonality>(emotes.Count);
        //filler
        foreach (NPCPersonality personality in emotePersonalities) 
        {
            personalityProducts.Add(new NPCPersonality());
        }
        int index = 0;
        float maxPlaneAffinityProduct = 0, maxExplorativityProduct = 0;
        foreach (NPCPersonality personality in emotePersonalities) // wait should we be multiplying?
        {
            personalityProducts[index].SetPlaneAffinity(1 - Mathf.Abs(personality.PlaneAffinity - planeAffinity));
            if (maxPlaneAffinityProduct < personalityProducts[index].PlaneAffinity) 
            {
                maxPlaneAffinityProduct = personalityProducts[index].PlaneAffinity;
            }
            personalityProducts[index].SetExplorativity(1 - Mathf.Abs(personality.Explorativity - explorativity));
            if (maxExplorativityProduct < personalityProducts[index].PlaneAffinity) 
            {
                maxExplorativityProduct = personalityProducts[index].Explorativity;
            }
            index++;
        }
        float[] meanSelectionScores = new float[emotes.Count];
        for (int i = 0; i < personalityProducts.Count; i++) 
        {
            //Normalize -- maybe not
            //personalityProducts[i].SetPlaneAffinity(personalityProducts[i].PlaneAffinity * (1 / maxPlaneAffinityProduct));
            //personalityProducts[i].SetExplorativity(personalityProducts[i].Explorativity * (1 / maxExplorativityProduct));
            //Calculate random-number selection scores (in polishing add a Gaussian integral transformation or something to get better accuracy) 
            float rand = Random.Range(0, 1);
            float planeAffinitySelectionScore = personalityProducts[i].PlaneAffinity - rand;
            float explorativitySelectionScore = personalityProducts[i].Explorativity - rand;
            meanSelectionScores[i] = (planeAffinitySelectionScore + explorativitySelectionScore) / 2f;
            //personalityProducts[i].SetMeanSelectionScore();
        }
        return System.Array.IndexOf(meanSelectionScores, meanSelectionScores.Max());
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(EmoteSequence());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
