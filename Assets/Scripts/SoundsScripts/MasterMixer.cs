using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MasterMixer : MonoBehaviour
{
    public AudioMixer mixer; // referencie o Audio Mixer no inspetor
    public Slider volumeSlider;

    void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float value)
    {
        mixer.SetFloat("MyExposedParam", value); // "Volume" é o nome do parâmetro exposto
    }
}
