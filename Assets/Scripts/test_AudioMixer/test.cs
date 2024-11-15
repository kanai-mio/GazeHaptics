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
        // �h���b�v�_�E���̓��e���N���A���A�񋓂����Đ��f�o�C�X��ǉ�
        dropdown.options.Clear();
        var availableOutputs = FMOD_SystemW.AvailableOutputs(LogLevel.DEBUG, gameObject.name, null);
        foreach (var availableOutput in availableOutputs)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(availableOutput.name));

        }
        // �h���b�v�_�E���̕ύX�C�x���g��ݒ�
        dropdown.onValueChanged.AddListener(OnValueChanged);

    }
    private void OnValueChanged(int value)
    {
        // AudioMixer�̍Đ��f�o�C�X��ݒ�
        audioMixer.SetFloat("OutputDevice ID", (float)value);

    }
}
