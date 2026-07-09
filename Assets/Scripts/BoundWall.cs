using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundWall : MonoBehaviour
{
    public PopupMessage popup;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        popup.message.text = "You've hit the wall.. please go forward :)";
        popup.message.duration = 1.5f;
    }
}
