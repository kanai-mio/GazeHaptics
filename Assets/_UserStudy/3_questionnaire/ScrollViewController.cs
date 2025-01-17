using UnityEngine;
using UnityEngine.UI;

public class ScrollViewController : MonoBehaviour
{
    public ScrollRect scrollRect; // ScrollRectをアタッチ
    public float scrollSpeed = 0.7f; // スクロール速度を調整

    void Update()
    {
        // サムスティックのY軸入力を取得
        float scrollInput = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y;

        // スクロールビューの位置を更新
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition += scrollInput * scrollSpeed * Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition); // 0~1の範囲に制限
        }

        //Debug.Log(scrollInput);
    }
}
