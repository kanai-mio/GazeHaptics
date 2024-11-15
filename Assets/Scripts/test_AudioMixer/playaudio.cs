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
    [SerializeField] List<string> deviceNames = new List<string>();         // �ėp�f�o�C�X�����X�g
    [SerializeField] int targetDeviceNameIndex;                             // �f�o�C�X�����X�g�̎g�p�C���f�b�N�X

    Dictionary<string, int> deviceDict = new Dictionary<string, int>();       // �f�o�C�X�R���N�V����(key:�f�o�C�X��, value:�f�o�C�X�ԍ�)

    [Header("Directory")]
    [SerializeField] string audDirPath = "\\Assets\\Audio";                  // �����f�B���N�g��

    [Header("Audio")]
    Dictionary<string, AudioUnit> audioDict = new Dictionary<string, AudioUnit>();      // �����R���N�V����

    /* Properties */
    /// <summary> �����R���N�V���� </summary>
    public Dictionary<string, AudioUnit> AudioDictionary => audioDict;

    //--------------------------------------------------
    // �������N���X
    public class AudioUnit
    {
        string audioFileName;      // �t�@�C����
        string audioFilePath;      // �t�@�C���p�X

        WaveOutEvent waveOut = new WaveOutEvent();  // �����f�[�^
        WaveFileReader reader;

        //------------------------------
        // �R���X�g���N�^
        public AudioUnit(string path, int deviceNum)
        {
            try
            {
                // �t�@�C�����Z�b�g
                audioFilePath = path;
                audioFileName = System.IO.Path.GetFileNameWithoutExtension(path);

                // WaveOut�̏�����
                waveOut.DeviceNumber = deviceNum;
                reader = new WaveFileReader(audioFilePath);
                waveOut.Init(reader);
            }

            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        // �f�X�g���N�^
        ~AudioUnit()
        {
            waveOut?.Dispose();
        }

        //------------------------------
        /// <summary> �������Đ����� </summary>
        public void PlayAudio()
        {
            reader.Position = 0;        // �Đ��ʒu���Z�b�g
            waveOut?.Play();

            print($"{audioFileName} was played");
        }
        public void StopAudio()
        {
            reader.Position = 0;        // �Đ��ʒu���Z�b�g
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
        // �L�[���͂ōĐ�
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
    // �w��t�H���_���̉����t�@�C���̃p�X���擾����
    string[] GetAudioFilePaths(string dirPath)
    {
        return System.IO.Directory.GetFiles(dirPath, "*.wav", System.IO.SearchOption.AllDirectories);
    }

    // �����f�[�^�Z�b�g
    void SetAudioUnits(string dirPath, string deviceName)
    {
        // �R���N�V�����̃��Z�b�g
        audioDict.Clear();
        deviceDict.Clear();

        // �f�o�C�X�擾
        deviceDict = GetDeviceNames();
        /*Debug.Log(deviceDict.Count);
        foreach(var pair in deviceDict)
        {
            Debug.Log(pair.Key + " " + pair.Value);
        }*/

        // �f�o�C�X�Ɋ܂܂�Ă��邩
        if (deviceDict.ContainsKey(deviceName))
        {
            // �t�@�C���̉������C���X�^���X���A�R���N�V�����ɒǉ�
            foreach (var filePath in GetAudioFilePaths(dirPath))
            {
                var audUnit = new AudioUnit(filePath, deviceDict[deviceName]);
                audioDict.Add(System.IO.Path.GetFileNameWithoutExtension(filePath), audUnit);
            }
        }

        else
        {
            throw new System.Exception("�w�肳�ꂽ�f�o�C�X�͑��݂��܂���");
        }
    }

    //--------------------------------------------------
    // �f�o�C�X���擾
    Dictionary<string, int> GetDeviceNames()
    {
        var devices = new Dictionary<string, int>();

        for (int i = 0; i < WaveOut.DeviceCount; i++)
        {
            var capabilities = WaveOut.GetCapabilities(i);

            // �f�o�C�X���̏d������
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
