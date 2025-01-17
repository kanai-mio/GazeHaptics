using UnityEngine;
using UnityEngine.UI;

public class ScrollViewController : MonoBehaviour
{
    public ScrollRect scrollRect; // ScrollRect���A�^�b�`
    public float scrollSpeed = 0.7f; // �X�N���[�����x�𒲐�

    void Update()
    {
        // �T���X�e�B�b�N��Y�����͂��擾
        float scrollInput = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y;

        // �X�N���[���r���[�̈ʒu���X�V
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition += scrollInput * scrollSpeed * Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition); // 0~1�͈̔͂ɐ���
        }

        //Debug.Log(scrollInput);
    }
}
