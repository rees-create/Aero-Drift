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

    void Start() 
    {
        Button btn = triggerButton.GetComponent<Button>();
        btn.onClick.AddListener(LoadNextLevel);
    }
    
    public void LoadNextLevel() 
    {
        Debug.Log("clicked Home");
        StartCoroutine(TransitionToScene(sceneName));
    }

    IEnumerator TransitionToScene(string sceneName) 
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(sceneName);
        
    }
}
