using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CollectibleCounter: MonoBehaviour
{
    public int collectibleCount;
    public TextMeshPro textMeshPro;
    public GameObject coinIcon;
    public string coinAnimBoolName;
    public string coinAnimName;

    int oldCollectibleCount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (collectibleCount > oldCollectibleCount)
        {
            oldCollectibleCount = collectibleCount;
            if (coinIcon != null)
            {
                coinIcon.GetComponent<Animator>().SetBool(coinAnimBoolName, true);
            }
        }
        else 
        {
            bool animDone = coinIcon.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
            bool isThisAnim = coinIcon.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(coinAnimName);
            if (animDone && isThisAnim) 
            {
                coinIcon.GetComponent<Animator>().SetBool(coinAnimBoolName, false);
            }
        }
        if(textMeshPro != null)
            textMeshPro.text = collectibleCount.ToString();
    }
}
