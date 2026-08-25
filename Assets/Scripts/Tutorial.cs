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
    }
    public TutorialObject currentTutorial;
    public List<TutorialObject> onLoadTutorials;

    float displayTime;
    public void DisplayTutorial() 
    {
        header.text = currentTutorial.title;
        body.text = currentTutorial.description;
        displayTime += Time.deltaTime;
        if (displayTime >= currentTutorial.delayTimeSeconds) 
        {
            currentTutorial = default;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
               
    }

    // Update is called once per frame
    void Update()
    {
        if (tutorialActive)
        {

        }
    }
}
