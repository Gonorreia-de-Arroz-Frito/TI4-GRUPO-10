using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional, se quiser que sobreviva a trocas de cena
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip, AudioMixerGroup mixerGroup = null)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
            
            audioSource.outputAudioMixerGroup = mixerGroup; // <<< configurar o mixer aqui
        }
    }
}
