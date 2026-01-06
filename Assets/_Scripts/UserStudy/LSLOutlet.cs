using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class EyeGazeLSLOutlet : MonoBehaviour
{
    // ストリームの基本情報
    public string StreamName = "UnityEyeGaze";
    public string StreamType = "Gaze";

    private StreamOutlet outlet;
    private float[] sample;   // 送るデータ配列（チャンネル数ぶん作る）

    void Start()
    {
        int channelCount = 2; // 例：GazeX, GazeY の2チャンネル

        // StreamInfo を作成
        StreamInfo streamInfo = new StreamInfo(
            StreamName,
            StreamType,
            channelCount,
            LSL.LSL.IRREGULAR_RATE,   // サンプリングレート未定義
            channel_format_t.cf_float32,
            System.Guid.NewGuid().ToString()
        );

        outlet = new StreamOutlet(streamInfo);
        sample = new float[channelCount];
    }

    void Update()
    {
        // ここにアイトラッキングのデータ取得処理を入れる
        // （例）画面上の正規化座標 0.0?1.0 を送る
        //Vector2 gaze = GetDummyGaze(); // あとで自分のデバイスの関数に差し替える

        Vector3 gaze = main_GazeHaptics.instance.hitPos;

        sample[0] = gaze.x;
        sample[1] = gaze.y;

        // LSL に送信
        outlet.push_sample(sample);
    }

    // テスト用：ランダムな視線データ
    private Vector2 GetDummyGaze()
    {
        return new Vector2(
            Mathf.Clamp01(Random.value),
            Mathf.Clamp01(Random.value)
        );
    }
}
