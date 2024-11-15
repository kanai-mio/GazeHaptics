using NAudio.Wave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playaudio : MonoBehaviour
{
    //audio
    public AudioSource audioSource1;
    public AudioSource audioSource2;

    public AudioClip audioClip1;
    public AudioClip audioClip2;


    [Header("Device")]
    [SerializeField] List<string> deviceNames = new List<string>();         // 汎用デバイス名リスト
    [SerializeField] int targetDeviceNameIndex;                             // デバイス名リストの使用インデックス

    Dictionary<string, int> deviceDict = new Dictionary<string, int>();       // デバイスコレクション(key:デバイス名, value:デバイス番号)

    [Header("Directory")]
    [SerializeField] string audDirPath = "\\Assets\\Audio";                  // 音声ディレクトリ

    [Header("Audio")]
    Dictionary<string, AudioUnit> audioDict = new Dictionary<string, AudioUnit>();      // 音声コレクション

    /* Properties */
    /// <summary> 音声コレクション </summary>
    public Dictionary<string, AudioUnit> AudioDictionary => audioDict;

    //--------------------------------------------------
    // 音声情報クラス
    public class AudioUnit
    {
        string audioFileName;      // ファイル名
        string audioFilePath;      // ファイルパス

        WaveOutEvent waveOut = new WaveOutEvent();  // 音声データ
        WaveFileReader reader;

        //------------------------------
        // コンストラクタ
        public AudioUnit(string path, int deviceNum)
        {
            try
            {
                // ファイル情報セット
                audioFilePath = path;
                audioFileName = System.IO.Path.GetFileNameWithoutExtension(path);

                // WaveOutの初期化
                waveOut.DeviceNumber = deviceNum;
                reader = new WaveFileReader(audioFilePath);
                waveOut.Init(reader);
            }

            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        // デストラクタ
        ~AudioUnit()
        {
            waveOut?.Dispose();
        }

        //------------------------------
        /// <summary> 音声を再生する </summary>
        public void PlayAudio()
        {
            reader.Position = 0;        // 再生位置リセット
            waveOut?.Play();

            print($"{audioFileName} was played");
        }
        public void StopAudio()
        {
            reader.Position = 0;        // 再生位置リセット
            waveOut?.Stop();

            print($"{audioFileName} was stoped");
        }
        public void VolumeUp()
        {
            print($"The former volume of {audioFileName} is" + waveOut.Volume);

            waveOut.Volume += 0.1f;

            print($"The later volume of {audioFileName} is" + waveOut.Volume);
        }
        public void VolumeDown()
        {
            print($"The former volume of {audioFileName} is" + waveOut.Volume);

            waveOut.Volume -= 0.1f;

            print($"The later volume of {audioFileName} is" + waveOut.Volume);
        }
    }

    private bool isPlaying;

    //--------------------------------------------------
    void Start()
    {
        var currentDirPath = System.IO.Directory.GetCurrentDirectory();

        SetAudioUnits(currentDirPath + audDirPath, deviceNames[targetDeviceNameIndex]);

        isPlaying = false;
    }

    private void Update()
    {
        // キー入力で再生
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isPlaying)
            {
               /* audioDict["acoustic_guitar"].PlayAudio();
                audioDict["rain"].PlayAudio();*/

                audioSource1.Play();
                audioSource2.Play();

                isPlaying = true;

                Debug.Log(isPlaying);
            }
            else
            {
               /* audioDict["acoustic_guitar"].StopAudio();
                audioDict["rain"].StopAudio();*/

                audioSource1.Stop();
                audioSource2.Stop();

                isPlaying = false;

                Debug.Log(isPlaying);
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            audioDict["acoustic_guitar"].VolumeUp();
            audioDict["rain"].VolumeUp();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            audioDict["acoustic_guitar"].VolumeDown();
            audioDict["rain"].VolumeDown();
        }
    }

    //--------------------------------------------------
    // 指定フォルダ内の音声ファイルのパスを取得する
    string[] GetAudioFilePaths(string dirPath)
    {
        return System.IO.Directory.GetFiles(dirPath, "*.wav", System.IO.SearchOption.AllDirectories);
    }

    // 音声データセット
    void SetAudioUnits(string dirPath, string deviceName)
    {
        // コレクションのリセット
        audioDict.Clear();
        deviceDict.Clear();

        // デバイス取得
        deviceDict = GetDeviceNames();
        /*Debug.Log(deviceDict.Count);
        foreach(var pair in deviceDict)
        {
            Debug.Log(pair.Key + " " + pair.Value);
        }*/

        // デバイスに含まれているか
        if (deviceDict.ContainsKey(deviceName))
        {
            // ファイルの音声をインスタンス化、コレクションに追加
            foreach (var filePath in GetAudioFilePaths(dirPath))
            {
                var audUnit = new AudioUnit(filePath, deviceDict[deviceName]);
                audioDict.Add(System.IO.Path.GetFileNameWithoutExtension(filePath), audUnit);
            }
        }

        else
        {
            throw new System.Exception("指定されたデバイスは存在しません");
        }
    }

    //--------------------------------------------------
    // デバイス名取得
    Dictionary<string, int> GetDeviceNames()
    {
        var devices = new Dictionary<string, int>();

        for (int i = 0; i < WaveOut.DeviceCount; i++)
        {
            var capabilities = WaveOut.GetCapabilities(i);

            // デバイス名の重複判定
            if (!devices.ContainsKey(capabilities.ProductName))
            {
                devices.Add(capabilities.ProductName, i);

                print(capabilities.ProductName);
            }
        }

        return devices;
    }


    // Start is called before the first frame update
    /*void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/
}
