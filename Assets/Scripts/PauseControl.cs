using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PauseControl : MonoBehaviour
{
    public Sprite playSprite;
    public Sprite pauseSprite;
    public GameObject pauseMenu;
    public float swipeDelta;
    public float swipeTime;

    Vector3 initPauseMenuPos;
    bool isPaused = false;
    bool tooQuick = false;

    public void PauseClicked()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f: 1f;
    }
    public void SwapSprite() 
    {
        if (gameObject.GetComponent<Image>() != null)
        {
            gameObject.gameObject.GetComponent<Image>().sprite = isPaused ? playSprite : pauseSprite;
        }
    }
    public void PauseMenu() 
    {
        //pauseMenu.SetActive(isPaused);
        //animator.SetBool("PauseMenuUp", isPaused);
        //animator.speed = isPaused ? 1: -1;
        if (isPaused)
        {
            StartCoroutine(PauseMenuSwipe(tooQuick ? initPauseMenuPos: pauseMenu.transform.localPosition, Vector3.zero));
        }
        else
        {
            //StartCoroutine(PauseMenuSwipe(initPauseMenuPos, Vector3.zero));
            StartCoroutine(PauseMenuSwipe(pauseMenu.transform.localPosition, initPauseMenuPos));
        }
    }
    IEnumerator PauseMenuSwipe(Vector3 init, Vector3 target)
    {
        float accumulation = 0f;
        float scaledDelta = swipeDelta * swipeTime;
        bool isPausedInit = isPaused;
        pauseMenu.SetActive(true);
        do
        {
            pauseMenu.transform.localPosition = Vector3.Lerp(init, target, accumulation);
            accumulation += swipeDelta;
            if (isPausedInit != isPaused) 
            {
                tooQuick = true;
                break;
            }
            //print(accumulation);
            yield return new WaitForSecondsRealtime(scaledDelta);
        } while (accumulation < 1f + swipeDelta);

        if (!tooQuick)
        {
            pauseMenu.SetActive(isPaused);
        }
        tooQuick = false;
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
        initPauseMenuPos = pauseMenu.transform.localPosition;
        pauseMenu.SetActive(isPaused);
        //print(initPauseMenuPos);
    }

}
