using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class LSLManager : MonoBehaviour
{
    public static LSLManager instance;

    public StreamOutlet markerOutlet;
    public StreamOutlet gazeOutlet;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも破棄しない
            SetupLSL();
        }
        else { Destroy(gameObject); }
    }

    void SetupLSL()
    {
        // マーカー用
        StreamInfo markerInfo = new StreamInfo(
            "UnityMarkers",
            "Markers",
            1,
            LSL.LSL.IRREGULAR_RATE,
            channel_format_t.cf_double64,
            "ID_Marker"
        );
        markerOutlet = new StreamOutlet(markerInfo);

        // アイトラッキング用 (2ch: x, y)
        StreamInfo gazeInfo = new StreamInfo(
            "UnityEyeGaze",
            "Gaze",
            2,
            LSL.LSL.IRREGULAR_RATE,   // サンプリングレート未定義
            channel_format_t.cf_float32,
            "ID_Gaze"
        );
        gazeOutlet = new StreamOutlet(gazeInfo);
    }
}
