using UnityEngine;
using AudioStream;
using AudioStreamSupport;
using UnityEngine.Audio;


public class main_GazeHaptics : MonoBehaviour
{
    public static main_GazeHaptics instance;

    OVREyeGaze eyeGaze;

    public Camera Camera;

    public Transform Head;


    //object hit bool
    //public bool hitBool { get; private set; }

    //gaze point object
    //public GameObject shape;

    //statas
    public bool isPlaying = false;
    //public bool pastBool = false;

    public int termNo;

    //Audio Mixer
    [SerializeField] private AudioMixer audioMixer;

    //audio
    public AudioSource[] audioSources;

    //haptic
    public AudioSource[] hapticSources;

    //haptic source position
    public Vector3[] hapticPoints;

    public int objectNum;

    //gaze point
    public Vector3 hitPos = Vector3.zero;
    private Vector3 lastGaze = Vector3.zero;

    //detect gaze point
    bool IntersectRayWithPlane(Vector3 rayOrigin, Vector3 rayDirection, out Vector3 hitPos)
    {
        hitPos = Vector3.zero;

        // ƒŒƒC‚ª•½–Ê‚Æ•½s‚©‚Ç‚¤‚©‚ðŠm”F
        if (Mathf.Approximately(rayDirection.z, 0))
        {
            return false; // •½s‚ÅŒð·‚µ‚È‚¢
        }

        // t ‚ðŒvŽZ
        float t = (5.0f - rayOrigin.z) / rayDirection.z;

        // t ‚ª³‚Ìê‡‚Ì‚ÝŒð“_‚ðŒvŽZiƒŒƒC‚Ì‘O•û‚Ì‚Ýj
        if (t >= 0)
        {
            hitPos = rayOrigin + t * rayDirection;
            return true;
        }

        return false; // ƒŒƒC‚ª•½–Ê‚ÌŒã•û‚ÉŒü‚¢‚Ä‚¢‚é
    }

    //U•‚ÌŒvŽZ
    void AdjustHapticAmplitude1(float[] distances)
    {
        for (int i = 0; i < objectNum; i++)
        {
            //float vol = Mathf.Exp(-distances[i]);
            float vol = 1 / (1 + distances[i] * distances[i]);
            hapticSources[i].volume = vol ;
            //Debug.Log("vol" + ": " + vol);
            //Debug.Log("vol" + i + ": " + hapticSources[i].volume);
        }
    }
    void AdjustHapticAmplitude2(float[] distances)
    {
        int minIndex = 0;   // Å¬’l‚ð’T‚·
        for (int i = 1; i < objectNum; i++)
        {
            if (distances[i] < distances[minIndex])
            {
                minIndex = i;
            }
        }
        for (int i = 0; i < objectNum; i++)
        {
            if (i == minIndex)
            {
                hapticSources[i].volume = 1;
            }
            else
            {
                hapticSources[i].volume = 0;
            }
            Debug.Log("vol" + i + ": " + hapticSources[i].volume);
        }
    }

    void AdjustHapticAmplitude(float[] distances)
    {
        if (termNo == 1)
        {
            AdjustHapticAmplitude1(distances);
        }
        else if (termNo == 2)
        {
            AdjustHapticAmplitude2(distances);
        }
        else if (termNo == 3)
        {
            for (int i = 0; i < objectNum; i++)
            {
                hapticSources[i].volume = 0.33f;
            }
        }
    }

    

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        if (IDdata.termNo != 0)
        {
            termNo = IDdata.termNo;
        }    

        //AudioSource output device setup
        var availableOutputs = FMOD_SystemW.AvailableOutputs(LogLevel.DEBUG, gameObject.name, null);

        string audioDeviceName = "Headphones (Oculus Virtual Audio Device)";
        int audioIndex = availableOutputs.FindIndex(d => d.name == audioDeviceName);
        audioMixer.SetFloat("audioOutputDeviceID", audioIndex);

        string hapticDeviceName = "スピーカー (Realtek(R) Audio)";
        int hapticIndex = availableOutputs.FindIndex(d => d.name == hapticDeviceName);
        audioMixer.SetFloat("hapticOutputDeviceID", hapticIndex);

        eyeGaze = GetComponent<OVREyeGaze>();
        //hitBool = false;
        isPlaying = false;

        lastGaze = Vector3.zero;

        /*foreach (AudioSource audioSource in audioSources)
        {
            audioSource.Play();
        }
        foreach (AudioSource hapticSource in hapticSources)
        {
            hapticSource.Play();
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(eyeGaze);
        if (eyeGaze == null) return;

        // ƒAƒCƒgƒ‰ƒbƒLƒ“ƒO‚Ì—LŒøŽž
        if (eyeGaze.EyeTrackingEnabled)
        {
            // Ž‹ü‚Ì“¯Šú
            Vector3 direction = (eyeGaze.transform.rotation * Vector3.forward).normalized;
            Ray ray = new Ray(Camera.transform.position, direction);
            //RaycastHit hit;
            //shape.transform.position = Camera.transform.position + direction * 3.0f;

            hitPos = Camera.transform.position + direction * 5.0f;

            //•½–Ê‚Æ‚ÌŒð·”»’è
            if (IntersectRayWithPlane(ray.origin, ray.direction, out hitPos))
            {
                float[] distances = new float[objectNum];
                for (int i = 0; i < objectNum; i++)
                {
                    distances[i] = Vector3.Distance(hapticPoints[i], hitPos);
                    Debug.Log("dis" + i + ": " + distances[i]);
                }

                Debug.Log("hitPos: " + hitPos);

                //shape.transform.position = hitPos;

                AdjustHapticAmplitude(distances);
            }
            else
            {
                for (int i = 0; i < objectNum; i++)
                {
                    hapticSources[i].volume = 0.0f;
                }
            }
        }

        if (lastGaze == Vector3.zero && hitPos != Vector3.zero && isPlaying == false)
        {
            foreach (AudioSource audioSource in audioSources)
            {
                audioSource.Play();
            }
            foreach (AudioSource hapticSource in hapticSources)
            {
                hapticSource.Play();
            }
            isPlaying = true;
        }

        lastGaze = hitPos;
    }
}

