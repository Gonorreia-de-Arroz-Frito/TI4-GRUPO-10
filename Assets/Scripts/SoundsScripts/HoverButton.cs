using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class HoverButton : MonoBehaviour, IPointerEnterHandler
{
    
    public AudioClip hoverSound;
    private AudioSource audioSource;

    public AudioMixerGroup mixerGroup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if(audioSource != null && mixerGroup != null){
            audioSource.outputAudioMixerGroup = mixerGroup;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

}
