using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class GameOver : MonoBehaviour
{
    public GameObject menu;
    public GameObject plane;
    public FloorControl floorControl;
    public float floorBoundTimeout;
    public float menuSwipeDelta;
    public float menuSwipeTime;
    public float gameOverWatcherDelta;
    public float floorThickness;
    public float floorPositionLeeway;
    public UnityEvent gameOver;
    Vector3 initPosition;
    IEnumerator GameOverMenuSwipe(Vector3 init, Vector3 target)
    {
        float accumulation = 0f;
        float menuScaledDelta = menuSwipeDelta * menuSwipeTime;
        do
        {
            menu.transform.localPosition = Vector3.Lerp(init, target, accumulation);
            accumulation += menuScaledDelta;
            yield return new WaitForSeconds(menuScaledDelta);
        } while (accumulation < 1f);
    }
    IEnumerator GameOverWatcher(float deltaTime) 
    {
        float timeOutAccum = 0f;
        while (true)
        {

            float floorY = floorControl.layers[floorControl.GetCurrentLayer()].floorY;
            //foreach (var layer in floorControl.layers) 
            //{
            //    if (layer.layerLevel == floorControl.GetCurrentLayer()) 
            //    {
            //        floorY = layer.floorY;
            //    }
            //}
            //print(floorY + (floorThickness / 2));
            if (Mathf.Abs(plane.transform.position.y - (floorY + (floorThickness/2))) < floorPositionLeeway)
            {
                timeOutAccum += deltaTime;
                print($"plane elevation = {plane.transform.position.y} plane on floor.. accumulating time: {timeOutAccum}");
            }
            else 
            {
                timeOutAccum = 0;
            }

            if (timeOutAccum >= floorBoundTimeout)
            {
                menu.SetActive(true);
                StartCoroutine(GameOverMenuSwipe(initPosition, Vector3.zero));
                gameOver.Invoke();
                break;  
            }

            yield return new WaitForSeconds(deltaTime);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        initPosition = menu.transform.localPosition;
        StartCoroutine(GameOverWatcher(gameOverWatcherDelta));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
