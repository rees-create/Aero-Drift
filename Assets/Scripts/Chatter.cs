using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Chatter : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> Npcs = new List<GameObject>();
    public float chatterDistance;
    [Range(0f, 1f)] public float chatterVolume;
    [Range(0f, 1f)] public float centroidFromFirstXRatio;
    public int updateCentroidAfterFrames;
    int frameCounter;
    bool invalidCentroid = false;



    float PlayAudioSourceAtVolume(AudioSource source, float volume)
    { 
        source.PlayOneShot(source.clip, volume);
        return source.clip.length;
    }
    
    void SortNPCsByDistance()
    {
        //foreach (GameObject npc in Npcs) 
        //{
            //Sort by distance to this object
        Npcs.Sort((a, b) =>
            Vector2.Distance(a.transform.position, transform.position)
            .CompareTo(Vector2.Distance(b.transform.position, transform.position))
        );
        //}
    }

    Vector2 FindChatterCentroid()
    {
        List<GameObject> chatterNpcs = new List<GameObject>();
        //Find NPCs in chatter radius
        int maxNPCsInList = (int)(centroidFromFirstXRatio * (float)Npcs.Count);
        foreach (GameObject npc in Npcs)
        {
            foreach (GameObject otherNpc in Npcs)
            {
                if (npc != otherNpc)
                {
                    if (Vector2.Distance(npc.transform.position, otherNpc.transform.position) < chatterDistance)
                    {
                        if (chatterNpcs.Count == maxNPCsInList)
                        {
                            break;
                        }
                        //check if all NPCs are within chatter distance of each other for npc before adding
                        if (!chatterNpcs.Contains(npc) && chatterNpcs.Count <= maxNPCsInList)
                        {
                            if (chatterNpcs.Count >= 1)
                            {
                                bool allClose = true;
                                foreach (GameObject addedNpc in chatterNpcs)
                                {
                                    if (!addedNpc.name.Equals(npc.name))
                                    {
                                        if (Vector2.Distance(addedNpc.transform.position, npc.transform.position) >= chatterDistance)
                                        {
                                            allClose = false;
                                            //print($"Not adding {npc.name} because it's too far from {addedNpc.name}");
                                            break;
                                        }
                                        else
                                        {
                                            //print($"Adding {npc.name} because it's close to {addedNpc.name}");
                                        }
                                    }
                                }
                                if (allClose) chatterNpcs.Add(npc);
                            }
                            else
                            {
                                chatterNpcs.Add(npc);
                            }
                        }
                        //check if all NPCs are within chatter distance of each other for otherNpc before adding
                        if (!chatterNpcs.Contains(otherNpc) && chatterNpcs.Count <= maxNPCsInList)
                        {
                            if (chatterNpcs.Count >= 1)
                            {
                                bool allClose = true;
                                foreach (GameObject addedNpc in chatterNpcs)
                                {
                                    if (!addedNpc.name.Equals(otherNpc.name))
                                    {
                                        if (Vector2.Distance(addedNpc.transform.position, otherNpc.transform.position) >= chatterDistance)
                                        {
                                            allClose = false;
                                            //print($"Not adding {otherNpc.name} because it's too far from {addedNpc.name}");
                                            break;
                                        }
                                        else
                                        {
                                            //print($"Adding {otherNpc.name} because it's close to {addedNpc.name}");
                                        }
                                    }
                                }
                                if (allClose) chatterNpcs.Add(otherNpc);
                            }
                            else
                            {
                                chatterNpcs.Add(otherNpc);
                            }
                        }
                    }
                }
            }
            if (chatterNpcs.Count == maxNPCsInList)
            {
                break;
            }
        }
        if (chatterNpcs.Count == 0)
        {
            invalidCentroid = true;
        }
        string npcListString = "";
        foreach(GameObject npc in chatterNpcs)
        {
            npcListString += npc.name + $" position: {npc.transform.position}: , ";
        }
        
        Vector2 centroid = Vector2.zero;
        //Calculate centroid
        foreach (GameObject npc in chatterNpcs)
        {
            centroid.x += npc.transform.position.x;
            centroid.y += npc.transform.position.y;
        }
        centroid.x /= (float)maxNPCsInList;
        centroid.y /= (float)maxNPCsInList;

        print($"MaxNPCs = {maxNPCsInList}; Chatter NPCs = {npcListString}; centroid = {centroid}");

        return centroid;
    }
    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        frameCounter++;
        if (frameCounter >= updateCentroidAfterFrames)
        {
            SortNPCsByDistance();
            Vector2 centroid = FindChatterCentroid();
            if (invalidCentroid)
            {
                frameCounter = 0;
                invalidCentroid = false;
            }
            else
            {
                transform.position = centroid;
                if (gameObject.GetComponent<AudioSource>() != null)
                {
                    PlayAudioSourceAtVolume(GetComponent<AudioSource>(), chatterVolume);
                }
                frameCounter = 0;
            }
        }
    }
}
