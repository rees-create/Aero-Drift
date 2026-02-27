using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine;

public class UpdateCoinAmount : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public int currentCoinAmount;
    public bool readFromTMPro;
    public bool resetCoinAmount;
    IEnumerator CoinUpdates() 
    {
        while (true) 
        {
            if (readFromTMPro) 
            {
                currentCoinAmount = int.Parse(textMeshPro.text);
            }
            if (resetCoinAmount) 
            {
                PlayerPrefs.SetInt("AmountOfCoins", 0);
            }
            yield return new WaitUntil(() => currentCoinAmount != PlayerPrefs.GetInt("AmountOfCoins"));
            currentCoinAmount = PlayerPrefs.GetInt("AmountOfCoins");
            textMeshPro.text = currentCoinAmount.ToString();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CoinUpdates());
    }

}
