using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public TextMeshProUGUI header, body;
    public Button skipTutorialButton;
    bool tutorialActive; //instant in which tutorial is active
    public bool tutorialEnabled;

    public void SetTutorialActive() 
    {
        tutorialActive = true;
    }
    public void DisableTutorial() 
    {
        tutorialActive = false;
    }
    public bool GetTutorialActive() 
    {
        return tutorialActive;
    }
    //tutorial should either..
    //A. listen for class "events" (like collecting your first coin)
    //or
    //B. other scripts should trigger tutorialActive with tutorial steps if (tutorialEnabled)
    //yeah I'm going with B for now.
    [Serializable] public struct TutorialObject
    {
        public string title;
        public string description;
        public float delayTimeSeconds;
        public float timeScale;
        public Vector2 objPosition;
        public Vector3 offset;
        public bool on;
    }
    public TutorialObject currentTutorial;
    public Queue<TutorialObject> onLoadTutorials;

    float displayTime;
    public void DisplayTutorial(TutorialObject tutorial) 
    {
        header.text = currentTutorial.title;
        body.text = currentTutorial.description;
        
        if (displayTime < 0.0001) 
        {
            print($"intended position: {transform.TransformPoint(currentTutorial.objPosition) + currentTutorial.offset}");
            transform.localPosition = transform.TransformPoint(currentTutorial.objPosition) + currentTutorial.offset;
        }
        
        displayTime += Time.deltaTime;
        //print($"displayTime {displayTime}");
        // if display time = 0, use a deactivation event or boolean to reset and dequeue in the next tutorial
        currentTutorial.on = true;
        if (onLoadTutorials != null && displayTime >= currentTutorial.delayTimeSeconds) 
        {
            currentTutorial = default;
            displayTime = 0f;
            //currentTutorial.on = false;
            if (onLoadTutorials.Count > 0 && currentTutorial.on == false)
            {
                currentTutorial = onLoadTutorials.Dequeue();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        onLoadTutorials = new Queue<TutorialObject>();
        if (onLoadTutorials != null)
        {
            if(onLoadTutorials.Count > 0)
            currentTutorial = onLoadTutorials.Dequeue();
            currentTutorial.on = true;
        }
        print($"currentTutorial: {currentTutorial.description}");
    }

    // Update is called once per frame
    void Update()
    {
        if (tutorialEnabled)
        {
            if (tutorialActive)
            {
                DisplayTutorial(currentTutorial);
                
            }
        }
    }
}
