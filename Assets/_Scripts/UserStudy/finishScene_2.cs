using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;



public class finishScene_2 : MonoBehaviour
{
    public class EyeGazeData
    {
        public float Time { get; set; }
        public int HitObject { get; set; }
        public float HitPosition_x { get; set; }
        public float HitPosition_y { get; set; }
        public float HitPosition_z { get; set; }

        // コンストラクタ
        public EyeGazeData(float time, float hitPosition_x, float hitPosition_y, float hitPosition_z)
        {
            Time = time;
            HitPosition_x = hitPosition_x;
            HitPosition_y = hitPosition_y;
            HitPosition_z = hitPosition_z;
        }
    }


    private static int userID = IDdata.userID;
    private static int termNo = IDdata.termNo;
    private static int Times = IDdata.Times;
    private List<EyeGazeData> dataList = new List<EyeGazeData>();

    private static string fileName = $"GazeData_ID{userID}_times{Times}_term{termNo}";
    private string filePath = @"C:\Users\mio\Desktop\userstudy\GazeData\" + fileName + ".csv";
    void CreateCSV()
    {
        //CSVファイルにanswersを出力
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // 固定値の書き込み
            writer.WriteLine($"ID,{userID},,,");
            writer.WriteLine($"Times,{Times},,,");
            writer.WriteLine($"No,{termNo},,,");
            //ヘッダー
            writer.WriteLine("Time,ObjectID,hitPos_x,hitPos_y,hitPos_z");
            // 動的データの書き込み
            foreach (var data in dataList)
            {
                writer.WriteLine($"{data.Time},{data.HitPosition_x},{data.HitPosition_y},{data.HitPosition_z}");
            }
        }

        Debug.Log($"CSVファイルが生成されました: {filePath}");
    }

    public float startTime;

    // Start is called before the first frame update
    void Start()
    {
        //isPlaying = true;
        startTime = Time.time + 5.0f;
    }

    // Update is called once per frame
    void Update()
    {
        float currentTime = Time.time;
        if (currentTime > startTime)
        {
            //int objectID = abatar_gazehaptics.instance.objectID;
            Vector3 hitPos = main_GazeHaptics.instance.hitPos;
            EyeGazeData newData = new EyeGazeData(currentTime, hitPos.x, hitPos.y, hitPos.z);
            dataList.Add(newData);
        }        

        float pastTime = currentTime - startTime;
        if (pastTime > 62)
        {
            CreateCSV();
            SceneManager.LoadScene("finishScene");
        }
    }
}