using AudioStream;
using AudioStreamSupport;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
public class test : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private TMP_Dropdown dropdown;
    private void Start()
    {
        // ドロップダウンの内容をクリアし、列挙した再生デバイスを追加
        dropdown.options.Clear();
        var availableOutputs = FMOD_SystemW.AvailableOutputs(LogLevel.DEBUG, gameObject.name, null);
        foreach (var availableOutput in availableOutputs)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(availableOutput.name));

        }
        // ドロップダウンの変更イベントを設定
        dropdown.onValueChanged.AddListener(OnValueChanged);

    }
    private void OnValueChanged(int value)
    {
        // AudioMixerの再生デバイスを設定
        audioMixer.SetFloat("OutputDevice ID", (float)value);

    }
}
