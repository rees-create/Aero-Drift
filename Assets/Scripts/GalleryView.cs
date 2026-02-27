using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GalleryView : MonoBehaviour
{
    [SerializeField] List<GameObject> slides;
    [SerializeField] List<string> scenes;
    [SerializeField] GameObject frame;
    [SerializeField] GameObject mask;
    [SerializeField] GameObject leftButton, rightButton;
    [SerializeField] string sortingLayer;
    [SerializeField] SlideSettings slideSettings;
    [SerializeField] ManualScaleSettings manualScaleSettings;


    bool leftClicked = false, rightClicked = false;
    int currentSlide = 0;
    bool spawnSlideRunning = false;
    //float elapsedTime = 0;
    GameObject slide_slot = default;

    [Serializable]
    public struct SlideSettings 
    {
        public float deltaTime;
        public float slideDistance, slideDuration;
        [NonSerialized] public float elapsedTime;
        //public float slideDistanceNudge;

        public float SlideSpeed() 
        {
            return slideDistance / slideDuration;
        }
    }
    [Serializable]
    public struct ManualScaleSettings 
    {
        public float maskSize;
        public float imageToFrameRatio;
        public Vector2 scrollButtonAspectRatio;
        public float scrollButtonOffset;
    }
    enum Aspect {None, Width, Height };

    Vector3 multiplyVectors(Vector3 a, Vector3 b) 
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
    Vector3 divideVectors(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }
    public string GetCurrentSceneName() 
    {
        return scenes[currentSlide];
    }
    IEnumerator Slide(GameObject[] slides, SlideSettings settings, int direction) 
    {
        for (int i = 0; i < (int) (settings.slideDuration / settings.deltaTime); i++)
        {
            yield return new WaitForSeconds(settings.deltaTime);
            settings.elapsedTime += settings.deltaTime;
            Vector3 nudge = Vector3.right * settings.deltaTime * settings.SlideSpeed() * (float) direction;
            foreach(GameObject slide in slides) 
            {
                slide.transform.position += nudge;
            }
        }
    }
    void ScaleToThisObject(ref GameObject child, GameObject parent, Vector2 scale = default, Vector2 relativeOffset = default, Aspect aspectRatioPreserve = Aspect.None) 
    {
        RectTransform galleryTransform = parent.GetComponent<RectTransform>();
        RectTransform childTransform = child.GetComponent<RectTransform>();
        float scaleToFrameX = galleryTransform.rect.width / childTransform.rect.width;
        float scaleToFrameY = galleryTransform.rect.height / childTransform.rect.height;

        
        if (aspectRatioPreserve == Aspect.Height) 
        {
            float aspectRatio = galleryTransform.rect.width / galleryTransform.rect.height;
            scaleToFrameX = aspectRatio * scaleToFrameY;
        }
        if (aspectRatioPreserve == Aspect.Width)
        {
            float aspectRatio = galleryTransform.rect.height / galleryTransform.rect.width;
            scaleToFrameY = aspectRatio * scaleToFrameX;
        }

        if (scale == default) 
        {
            scale = Vector2.one;
        }
        if (relativeOffset == default) 
        {
            relativeOffset = Vector2.zero;
        }

        child.transform.localScale *= scale;//multiplyVectors(child.transform.TransformVector(child.transform.localScale), 
            //new Vector3(scaleToFrameX, scaleToFrameY) * scale);
        
        child.transform.position += new Vector3(relativeOffset.x, relativeOffset.y, 0);
    }
    bool checkClicked() { return leftClicked || rightClicked; }
    IEnumerator SpawnSlide() 
    {
        //Instantiate
        spawnSlideRunning = true;

        GameObject[] slides = { default, default };      
        //wait until click
        //yield return new WaitUntil(checkClicked);
        int nextSlide = currentSlide;
        if (rightClicked)
        {
            nextSlide = mod(currentSlide + 1, this.slides.Count);
            
        }
        else
        {
            nextSlide = mod(currentSlide - 1, this.slides.Count);
            
        }

        slides[0] = Instantiate(this.slides[currentSlide], mask.transform);
        ScaleToThisObject(ref slides[0], mask, Vector2.one * manualScaleSettings.imageToFrameRatio, aspectRatioPreserve: Aspect.Height);

        slides[1] = Instantiate(this.slides[nextSlide], mask.transform);

        int displacementArrow = rightClicked ? 1 : -1;
        Vector3 displacementVector = default;
        int slideDArrow = displacementArrow;
        if (transform.position.x > 0)
        {
            displacementVector = Vector3.right;
            slideDArrow = -displacementArrow;
        }
        else if (transform.position.x < 0) 
        {
            displacementVector = Vector3.left;
        }
        
        ScaleToThisObject(ref slides[1], mask, Vector2.one * manualScaleSettings.imageToFrameRatio, 
            displacementVector * slideSettings.slideDistance * slideDArrow, Aspect.Height);
        
        //animate
        IEnumerator slideAnim = Slide(slides, slideSettings, displacementArrow);
        yield return StartCoroutine(slideAnim);
        //now delete slide
        DestroyImmediate(slides[0]);
        DestroyImmediate(slides[1]);

        slide_slot = Instantiate(this.slides[nextSlide], mask.transform);
        ScaleToThisObject(ref slide_slot, mask, Vector2.one * manualScaleSettings.imageToFrameRatio, aspectRatioPreserve: Aspect.Height);

        slideSettings.elapsedTime = 0;
        leftClicked = false;
        rightClicked = false;

        spawnSlideRunning = false;
        
    }
    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
    void OnClickLeft()
    {
        DestroyImmediate(slide_slot); //remove from hierarchy
        slide_slot = default;
        
        leftClicked = true;
        if (!spawnSlideRunning)
        {
            StartCoroutine(SpawnSlide());
        }
        currentSlide = mod(currentSlide - 1, slides.Count);
    }
    void OnClickRight()
    {
        DestroyImmediate(slide_slot); //remove slide slot from hierarchy
        slide_slot = default;

        rightClicked = true;
        if (!spawnSlideRunning)
        {
            StartCoroutine(SpawnSlide());
        }
        currentSlide = mod(currentSlide + 1 , slides.Count);
    }

    void Start()
    {
        GameObject frame = Instantiate(this.frame, gameObject.transform);
        ScaleToThisObject(ref frame, gameObject);
        
        mask = Instantiate(mask, frame.transform);
        ScaleToThisObject(ref mask, frame, Vector2.one * manualScaleSettings.maskSize);
        
        GameObject angleArrowRight = Instantiate(rightButton, gameObject.transform);
        ScaleToThisObject(ref angleArrowRight, gameObject, manualScaleSettings.scrollButtonAspectRatio, Vector2.right * manualScaleSettings.scrollButtonOffset);
        GameObject angleArrowLeft = Instantiate(leftButton, gameObject.transform);
        ScaleToThisObject(ref angleArrowLeft, gameObject, manualScaleSettings.scrollButtonAspectRatio, Vector2.left * manualScaleSettings.scrollButtonOffset);
        
        slide_slot = Instantiate(slides[currentSlide], mask.transform);
        ScaleToThisObject(ref slide_slot, gameObject, Vector2.one * manualScaleSettings.imageToFrameRatio, aspectRatioPreserve: Aspect.Height);
        
        angleArrowRight.GetComponent<Button>().onClick.AddListener(OnClickRight);
        angleArrowLeft.GetComponent<Button>().onClick.AddListener(OnClickLeft);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
