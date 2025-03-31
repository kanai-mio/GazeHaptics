using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public static class VolumeData
{
    public static float[] volumes = new float[3];
}

public class tuning : MonoBehaviour
{
    public OVRInput.Button triggerButton = OVRInput.Button.PrimaryIndexTrigger;
    private Toggle currentToggle;
    //private Button currentButton;

    public Text header;
    public Text message;

    [SerializeField] public Toggle[] Options;
    [SerializeField] public Button[] PlusButtons;
    [SerializeField] public Button[] MinusButtons;
    public Text[] plusText;
    public Text[] minusText;

    public AudioSource[] hapticSources;

    public Button submitButton;

    LineRenderer linerend;

    public int number;

    void OnButtonClick()
    {
        for (int i = 0; i < 3; i++)
        {
            VolumeData.volumes[i] = hapticSources[i].volume;
        }
        CreateCSV();
        if(number < 6)
        {
            SceneManager.LoadScene($"tuning{number+1}");
        }
        else
        {
            SceneManager.LoadScene($"last");
        }
    }

    void IncreaseVolume(AudioSource hapticSource)
    {
        if(hapticSource.volume == 1.0f)
        {
            message.text = "You can't increase volume.";
        }
        else
        {
            hapticSource.volume = Mathf.Clamp(hapticSource.volume + 0.05f, 0.0f, 1.0f);
            message.text = "";
            Debug.Log("Volume Increased: " + hapticSource.volume);
        }
    }
    void DecreaseVolume(AudioSource hapticSource)
    {
        if (hapticSource.volume == 0.0f)
        {
            message.text = "You can't decrease volume.";
        }
        else
        {
            hapticSource.volume = Mathf.Clamp(hapticSource.volume - 0.05f, 0.0f, 1.0f);
            message.text = "";
            Debug.Log("Volume Decreased: " + hapticSource.volume);
        }
    }

    void Start()
    {
        submitButton.onClick.AddListener(OnButtonClick);
        header.text = $"Tuning ({number}/6)";
    }

    private void Update()
    {
        for (int i = 0; i < 3; i++)
        {
            if (Options[i].isOn)
            {
                if (!hapticSources[i].isPlaying)
                {
                    hapticSources[i].Play();
                    message.text = "";
                }
                if (i == 0)
                {
                    message.text = "You can't change volume.";
                }
                else
                {
                    if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickUp))
                    {
                        IncreaseVolume(hapticSources[i]); 
                        plusText[i].color = Color.red;
                    }
                    if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickDown))
                    {
                        DecreaseVolume(hapticSources[i]);
                        minusText[i].color = Color.red;
                    }
                    if (OVRInput.GetUp(OVRInput.RawButton.RThumbstickUp))
                    {
                        plusText[i].color = Color.black;
                    }
                    if (OVRInput.GetUp(OVRInput.RawButton.RThumbstickDown))
                    {
                        minusText[i].color = Color.black;
                    }
                }
            }
            else
            {
                if (hapticSources[i].isPlaying)
                {
                    hapticSources[i].Stop();
                }
            }
        }

        //レイキャスト
        Vector3 direction = (OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward).normalized;
        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch), direction);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000.0f))
        {
            //トグル
            Toggle toggle = hit.transform.GetComponent<Toggle>();
            if (toggle != null && toggle != currentToggle)
            {
                currentToggle = toggle;
                //Debug.Log("hit");
            }
            else
            {
                currentToggle = null;
            }
            if (OVRInput.GetDown(triggerButton) && currentToggle != null)
            {
                currentToggle.isOn = !currentToggle.isOn;
                //Debug.Log("pushed");
            }

            //ボタン
            Button button = hit.transform.GetComponent<Button>();
            if (button != null && OVRInput.GetDown(triggerButton))
            {
                button.onClick.Invoke();
            }
            
        }

        Debug.DrawRay(ray.origin, ray.direction * 15, Color.white);

        //LineRendererコンポーネントの取得
        linerend = this.GetComponent<LineRenderer>();

        linerend.startColor = Color.red;  // 始点の色
        linerend.endColor = Color.blue;  // 終点の色

        //線の太さを設定
        linerend.startWidth = 0.04f;
        linerend.endWidth = 0.04f;

        //始点, 終点を設定し, 描画
        linerend.SetPosition(0, ray.origin);
        linerend.SetPosition(1, ray.direction * 1000);
    }

    public int userID;
    private static string[] instruments = new string[] { "drum", "piano", "buss" };
    private static string fileName;
    private string filePath;

    void CreateCSV()
    {
        fileName = $"VolumeData_ID{userID}_scene{number}";
        filePath = @"C:\Users\mio\Desktop\userstudy\volumeData\" + fileName + ".csv";

        //CSVファイルに出力
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            /*// 固定値の書き込み
            writer.WriteLine($"ID,{userID}");
            writer.WriteLine($"Term,{termNo}");

            // 動的データの書き込み
            for (int i = 0; i < hapticSources.Length; i++)
            {
                writer.WriteLine($"{instruments[i]},{VolumeData.volumes[i]}");
            }*/

            writer.WriteLine($"{VolumeData.volumes[0]},{VolumeData.volumes[1]},{VolumeData.volumes[2]}");
        }
        Debug.Log($"CSVファイルが生成されました: {filePath}");
    }

}
