using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class changeText : MonoBehaviour
{
    public Text targetText;

    void Start()
    {
        if(IDdata.isVibration)
        {
            StartCoroutine(ChangeText_WB());
        }
        else
        {
            StartCoroutine(ChangeText_NB());
        }
    }

    IEnumerator ChangeText_WB()
    {
        yield return new WaitForSeconds(3f);
        targetText.text = "終了しました\r\nHMDを外してください";
    }

    IEnumerator ChangeText_NB()
    {
        yield return new WaitForSeconds(3f);
        targetText.text = "終了しました";
    }
}

