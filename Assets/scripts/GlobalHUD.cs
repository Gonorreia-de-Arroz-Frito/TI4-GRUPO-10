using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalHUD : MonoBehaviour
{
    public static GlobalHUD Instance;

    void Awake()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Impede o HUD de existir na cena de título
        if (currentScene == "Main Menu")
        {
            Destroy(gameObject);
            return;
        }
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
