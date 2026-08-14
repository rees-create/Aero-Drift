using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking.PlayerConnection;
using System;
using UnityEngine.UI;
using System.Data;

[RequireComponent(typeof(TextMeshProUGUI))]

public class PopupMessage : MonoBehaviour
{
    TextMeshProUGUI tmp;
    [Serializable] public struct Message 
    {
        public string text;
        public float duration;
        Vector2 normalizedScreenCoords;
        [NonSerialized] public bool active;
        
    }
    public Message message;
    
    [Serializable] public struct MessageBoxLayoutPreview {
        public Vector2 messageBoxTopLeft;
        public Vector2 messageBoxBottomRight;
        public Vector2 dot;
    }
    [SerializeField] MessageBoxLayoutPreview preview;
    public Image popUpPad;
    public Color padColor;

    // Start is called before the first frame update
    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    bool msgUpdateActive = false;
    bool[] msgUpdateActiveBuffer = new bool[2];
    int bufferIndex = 0;
    float initMsgDuration;
    void Update()
    {
        //buffer code to catch message params (just duration for now) when message is initialized.. hate writing this type of code
        //TODO: something here is likely freezing the pop up message fade
        
        msgUpdateActiveBuffer[bufferIndex] = msgUpdateActive;
        bufferIndex = (bufferIndex + 1) % 2;
        //check buffer, catch params
        if (msgUpdateActiveBuffer[0] != msgUpdateActiveBuffer[1]) 
        {
            initMsgDuration = message.duration;
        }

        if (tmp)
        {
            if (message.text.Length > 0 && message.duration > 0)
            {
                msgUpdateActive = true;
                if (tmp.text.Length == 0)
                {
                    tmp.text = message.text;
                }
                message.duration -= Time.deltaTime;
                //pad color
                float alphaCurve = 1.2f * Mathf.Sin((Mathf.PI/2) * (message.duration/initMsgDuration));
                padColor.a = alphaCurve <= 1 ? alphaCurve : 1;
                //print($"array = [{msgUpdateActiveBuffer[0]}, {msgUpdateActiveBuffer[1]}], padColor unclamped = {1.2f * Mathf.Sin((Mathf.PI / 2) * (message.duration / initMsgDuration))}");
                popUpPad.color = padColor;

            }
            else if (message.duration <= 0)
            {
                message.duration = 0; // message duration can't be under 0
                message.text = tmp.text = ""; //clear message
                msgUpdateActive = false;
                //msgUpdateActive = false;
            }
            else
            {
               
            }
        }
    }

    public void OnDrawGizmos()
    {
        Vector3 messageAreaSize = preview.messageBoxTopLeft - preview.messageBoxBottomRight;
        Vector3 center = Vector2.Lerp(preview.messageBoxTopLeft, preview.messageBoxBottomRight, 0.5f);
        Gizmos.color = new Vector4(0.6f, 0.851f, 0.918f, 1); //turquoise
        Gizmos.DrawWireCube(transform.TransformPoint(center), messageAreaSize);
        Gizmos.DrawSphere(transform.TransformPoint(preview.dot), 0.5f);
    }

}
