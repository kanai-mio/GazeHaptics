using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.IO;

public class Raycast : MonoBehaviour
{
    public OVRInput.Button triggerButton = OVRInput.Button.PrimaryIndexTrigger; // �g���K�[�{�^��
    private Toggle currentToggle;

    public Text header;

    public Toggle[] Options1;
    public Toggle[] Options2;
    public Toggle[] Options3;
    public Toggle[] Options4;
    public Button submitButton;
    public TMP_Text buttonText;

    private static int userID = IDdata.userID;
    private static int termNo = IDdata.termNo;

    private int[] answers = new int[] { 0, 0, 0, 0 };

    public Vector3 hitPos;

    LineRenderer linerend;

    private static string fileName = $"ID{userID}_term{termNo}";
    private string filePath = @"C:\Users\mio\Desktop\userstudy\" + fileName + ".csv";

    void submitAnswer()
    {
        for (int i = 0; i < 7; i++)
        {
            if (Options1[i].isOn)
            {
                answers[0] = i + 1;
            }
            if (Options2[i].isOn)
            {
                answers[1] = i + 1;
            }
            if (Options3[i].isOn)
            {
                answers[2] = i + 1;
            }
            if (Options4[i].isOn)
            {
                answers[3] = i + 1;
            }
        }

        if (allChecked())
        {
            //CSV�t�@�C����answers���o��
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // �Œ�l�̏�������
                writer.WriteLine($"ID,{userID}");
                writer.WriteLine($"No,{termNo}");

                // ���I�f�[�^�̏�������
                for (int i = 0; i < answers.Length; i++)
                {
                    writer.WriteLine($"{i+1},{answers[i]}");
                }
            }

            Debug.Log($"CSV�t�@�C������������܂���: {filePath}");

            Debug.Log($"answers: {answers[0]}, {answers[1]}, {answers[2]}, {answers[3]}");
            SceneManager.LoadScene("text_2");
        }
        else
        {
            
        }
    }

    void OnButtonClick()
    {
        Debug.Log("pushed button");
        submitAnswer();
    }

    private bool allChecked()
    {
        foreach (int ans in answers)
        {
            if (ans == 0)
            {
                return false;
            }
        }
        return true;
    }

    private void Start()
    {
        header.text = "Questionnaire";
        submitButton.onClick.AddListener(OnButtonClick);
        if (buttonText != null)
        {
            buttonText.text = "Submit";
        }

        Debug.Log($"ID:{userID}, No:{termNo}");
    }

    void Update()
    {
        Vector3 direction = (OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward).normalized;
        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch), direction);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000.0f))
        {
            //�g�O��
            Toggle toggle = hit.transform.GetComponent<Toggle>();
            if (toggle != null && toggle != currentToggle)
            {
                currentToggle = toggle;
                Debug.Log("hit");    
            }
            if (OVRInput.GetDown(triggerButton) && currentToggle != null)
            {
                currentToggle.isOn = !currentToggle.isOn;
                Debug.Log("pushed");
            }

            //�{�^��
            Button button = hit.transform.GetComponent<Button>();
            if (button != null)
            {
                currentToggle = null;
                if (OVRInput.GetDown(triggerButton))
                {
                    OnButtonClick();
                }
            }

        }


        Debug.DrawRay(ray.origin, ray.direction * 15, Color.red);

        //LineRenderer�R���|�[�l���g�̎擾
        linerend = this.GetComponent<LineRenderer>();

        //���̑�����ݒ�
        linerend.startWidth = 0.04f;
        linerend.endWidth = 0.04f;

        //�n�_, �I�_��ݒ肵, �`��
        linerend.SetPosition(0, ray.origin);
        linerend.SetPosition(1, ray.direction * 1000);
    }
}







