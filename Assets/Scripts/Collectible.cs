using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    //just trigger the animation once collided with by a player, wait till the end, then destroy the object
    // Start is called before the first frame update
    IEnumerator WaitForCollect() 
    {
        if (GetComponent<BoxCollider2D>() != null) 
        {
            //yield return new WaitUntil(());
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
