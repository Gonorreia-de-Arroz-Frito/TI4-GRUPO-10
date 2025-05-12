using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class ClickSound : MonoBehaviour
{
    public AudioClip clickSound;

    public AudioMixerGroup mixerGroup;
    
    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }

    }

    void PlayClickSound()
    {
        if (clickSound != null && mixerGroup != null)
        {
            AudioManager.Instance.PlaySound(clickSound, mixerGroup);
        }
    }
}
