using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class VolumeScore : MonoBehaviour
{
    public OVRInput.Button triggerButton = OVRInput.Button.PrimaryIndexTrigger;
    private Slider currentSlider;

    public Text[] scores;

    [SerializeField] public Slider[] sliders; // InspectorでSliderをアタッチ
    public float[] sliderValues;

    public Button submitButton;

    LineRenderer linerend;

    private static int userID = IDdata.userID;
    private static int termNo = IDdata.termNo;
    private static int Times = IDdata.Times;
    private static string[] instruments = new string[] { "drum", "piano"};
    private static string fileName = $"VolumeScore_ID{userID}_times{Times}_term{termNo}";
    private string filePath = @"C:\Users\mio\Desktop\userstudy\volumeScore\" + fileName + ".csv";
    void CreateCSV()
    {
        //CSVファイルにanswersを出力
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // 固定値の書き込み
            writer.WriteLine($"ID,{userID}");
            writer.WriteLine($"Times,{Times}");
            writer.WriteLine($"Term,{termNo}");

            // 動的データの書き込み
            for (int i = 0; i < scores.Length; i++)
            {
                writer.WriteLine($"{instruments[i]},{scores[i].text}");
            }
        }
        Debug.Log($"CSVファイルが生成されました: {filePath}");
    }

    void OnButtonClick()
    {
        Debug.Log("pushed button");
        CreateCSV();
        SceneManager.LoadScene("last");
    }

    // Start is called before the first frame update
    void Start()
    {
        submitButton.onClick.AddListener(OnButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            if (sliders[i] != null)
            {
                scores[i].text = $"{sliders[i].value}";
            }
        }

        Vector3 direction = (OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward).normalized;
        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch), direction);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000.0f))
        {
            //スライダー
            Slider slider = hit.transform.GetComponent<Slider>();
            if (slider != null)
            {
                currentSlider = slider;

                // トリガーボタンを押している間、スライダーを操作
                if (OVRInput.Get(triggerButton))
                {
                    UpdateSliderValue(ray, currentSlider, hit);
                }
            }
            else
            {
                currentSlider = null;
            }

            //ボタン
            Button button = hit.transform.GetComponent<Button>();
            if (button != null)
            {
                if (OVRInput.GetDown(triggerButton))
                {
                    button.onClick.Invoke();
                }
            }
        }

        //LineRendererコンポーネントの取得
        linerend = this.GetComponent<LineRenderer>();

        //線の太さを設定
        linerend.startWidth = 0.04f;
        linerend.endWidth = 0.04f;

        //始点, 終点を設定し, 描画
        linerend.SetPosition(0, ray.origin);
        linerend.SetPosition(1, ray.direction * 1000);
    }

    private void UpdateSliderValue(Ray ray, Slider slider, RaycastHit hit)
    {
        // ヒット位置のローカル座標を取得
        RectTransform rectTransform = slider.GetComponent<RectTransform>();
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                Camera.main.WorldToScreenPoint(hit.point),
                Camera.main,
                out localPoint))
        {
            // スライダーの幅を基に値を計算
            float newValue = Mathf.Clamp(((localPoint.x - rectTransform.rect.xMin) / rectTransform.rect.width) * 800f, 0.0f, 800f);
            slider.value = Mathf.CeilToInt(newValue);
        }
    }

}
