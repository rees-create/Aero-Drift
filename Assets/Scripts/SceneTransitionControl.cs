using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionControl : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;
    public Button triggerButton;
    public string sceneName;
    public GalleryView sceneGallery;
    public bool useSceneGallery;

    void Start() 
    {
        if (triggerButton != null)
        {
            Button btn = triggerButton.GetComponent<Button>();
            btn.onClick.AddListener(LoadNextLevel);
        }
    }
    
    public void LoadNextLevel() 
    {
        if (useSceneGallery) 
        {
            StartCoroutine(TransitionToScene(sceneGallery.GetCurrentSceneName()));
        }
        else 
        {
            StartCoroutine(TransitionToScene(sceneName));
        }
    }

    IEnumerator TransitionToScene(string sceneName) 
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(sceneName);
        
    }
}
