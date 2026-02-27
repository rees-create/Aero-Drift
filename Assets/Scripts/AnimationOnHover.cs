using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimationOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public GameObject targetObject; // The object to animate
    public string animBoolName;
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetObject.GetComponent<Animator>().SetBool(animBoolName, true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        targetObject.GetComponent<Animator>().SetBool(animBoolName, false);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
