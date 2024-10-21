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
    public List<GameObject> slides;
    public GameObject frame;
    public GameObject mask;
    public GameObject leftButton, rightButton;
    public SlideSettings slideSettings;
    public ManualScaleSettings manualScaleSettings;


    bool leftClicked = false, rightClicked = false;
    int currentSlide = 0;
    bool spawnSlideRunning = false;
    //float elapsedTime = 0;
    GameObject slide_slot = default;

    [Serializable]
    public struct SlideSettings 
    {
        public float deltaTime;
        public float slideSpeed, slideDuration;
        public float elapsedTime;
        public float slideDistanceNudge;

        public float SlideDistance() 
        {
            return (slideDuration * slideSpeed / deltaTime) + slideDistanceNudge;
        }
    }
    [Serializable]
    public struct ManualScaleSettings 
    {
        public float maskSize;
        public float imageToFrameRatio;
        public Vector2 scrollButtonAspectRatio;
        public int scrollButtonOffset;
    }
    enum Aspect {None, Width, Height };

    IEnumerator Slide(GameObject[] slides, SlideSettings settings, int direction) 
    {
        while (Math.Abs(settings.elapsedTime) < settings.slideDuration)
        {
            yield return new WaitForSeconds(settings.deltaTime);
            settings.elapsedTime += settings.deltaTime;
            Vector3 nudge = Vector3.right * settings.deltaTime * settings.slideSpeed * direction;
            foreach(GameObject slide in slides) 
            {
                slide.transform.position += nudge;
            }
        }
    }
    void ScaleToThisObject(ref GameObject child, Vector2 scale = default, Vector2 relativeOffset = default, Aspect aspectRatioPreserve = Aspect.None) 
    {
        RectTransform galleryTransform = GetComponent<RectTransform>();
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

        child.transform.localScale *= new Vector2(scaleToFrameX, scaleToFrameY) * scale;
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

        slides[0] = Instantiate(this.slides[currentSlide], gameObject.transform);
        ScaleToThisObject(ref slides[0], Vector2.one * 0.8f, aspectRatioPreserve: Aspect.Height);

        slides[1] = Instantiate(this.slides[nextSlide], gameObject.transform);

        int displacementArrow = rightClicked ? 1 : -1;
        ScaleToThisObject(ref slides[1], Vector2.one * manualScaleSettings.imageToFrameRatio, 
            transform.TransformPoint(Vector3.left * slideSettings.SlideDistance()) * displacementArrow, Aspect.Height);
        
        //animate
        IEnumerator slideAnim = Slide(slides, slideSettings, displacementArrow);
        yield return StartCoroutine(slideAnim);
        //now delete slide
        DestroyImmediate(slides[0]);
        DestroyImmediate(slides[1]);

        slide_slot = Instantiate(this.slides[nextSlide], gameObject.transform);
        ScaleToThisObject(ref slide_slot, Vector2.one * manualScaleSettings.imageToFrameRatio, aspectRatioPreserve: Aspect.Height);

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
        //print($"currentSlide = {currentSlide} slides.Count = {slides.Count} final = {(currentSlide - 1) % slides.Count}");
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
        //slide_slot = Instantiate(slides[currentSlide], gameObject.transform);
        //ScaleToThisObject(ref slide_slot, Vector2.one * 0.8f);
    }

    void Start()
    {
        GameObject mask = Instantiate(this.mask, gameObject.transform);
        ScaleToThisObject(ref mask, Vector2.one * manualScaleSettings.maskSize);
        GameObject frame = Instantiate(this.frame, gameObject.transform);
        ScaleToThisObject(ref frame);
        GameObject angleArrowRight = Instantiate(rightButton, gameObject.transform);
        ScaleToThisObject(ref angleArrowRight, manualScaleSettings.scrollButtonAspectRatio, Vector2.right * manualScaleSettings.scrollButtonOffset);
        GameObject angleArrowLeft = Instantiate(leftButton, gameObject.transform);
        ScaleToThisObject(ref angleArrowLeft, manualScaleSettings.scrollButtonAspectRatio, Vector2.left * manualScaleSettings.scrollButtonOffset);
        //StartCoroutine(SpawnSlide());
        slide_slot = Instantiate(slides[currentSlide], gameObject.transform);
        ScaleToThisObject(ref slide_slot, Vector2.one * manualScaleSettings.imageToFrameRatio, aspectRatioPreserve: Aspect.Height);
        
        angleArrowRight.GetComponent<Button>().onClick.AddListener(OnClickRight);
        angleArrowLeft.GetComponent<Button>().onClick.AddListener(OnClickLeft);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
