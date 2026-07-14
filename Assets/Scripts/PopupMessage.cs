using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking.PlayerConnection;
using System;
using UnityEngine.UI;

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
    void Update()
    {
        if (tmp)
        {
            if (message.text.Length > 0 && message.duration > 0)
            {
                if (tmp.text.Length == 0)
                {
                    tmp.text = message.text;
                }
                message.duration -= Time.deltaTime;
                //pad color
                float alphaCurve = 1.2f * Mathf.Sin(Mathf.PI * Time.deltaTime);
                padColor.a = alphaCurve <= 1 ? alphaCurve : 1;
                popUpPad.color = padColor;
            }
            else if (message.duration < 0)
            {
                message.duration = 0; // message duration can't be under 0
            }
            else
            {
                message.text = tmp.text = ""; //clear message
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
