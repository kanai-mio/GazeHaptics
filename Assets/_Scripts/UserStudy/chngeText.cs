using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class changeText : MonoBehaviour
{
    public Text targetText;

    void Start()
    {
        StartCoroutine(ChangeText());
    }

    IEnumerator ChangeText()
    {
        yield return new WaitForSeconds(10f);
        targetText.text = "終了しました\r\nHMDを外してください";
    }
}

