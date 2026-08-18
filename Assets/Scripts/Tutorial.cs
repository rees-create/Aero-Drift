using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public TextMeshProUGUI header, body;
    public Button skipTutorialButton;
    public bool tutorialActive; //instant in which tutorial is active
    public bool tutorialEnabled;
    //tutorial should either..
    //A. listen for class "events" (like collecting your first coin)
    //or
    //B. other scripts should trigger tutorialActive with tutorial steps if (tutorialEnabled)
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
