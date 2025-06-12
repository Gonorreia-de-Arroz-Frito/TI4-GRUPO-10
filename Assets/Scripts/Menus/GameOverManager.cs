using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    // Singleton instance
    public static GameOverManager Instance { get; private set; }

    public GameObject gameOverScreen;
    public TextMeshProUGUI gameOverMessage;

    private string[] mensagensLatinas = {
        "Mors est finis vitae",
        "Tenebris et tenebrarum",
        "Mortui in carne",
        "In tenebris ambulamus"
    };

    private void Awake()
    {
        // Implementação do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Garante que só há uma instância
        }
        else
        {
            Instance = this;
            // Remova DontDestroyOnLoad(gameObject); se a cena Game Over é carregada
        }
    }
    
    // Start é chamado antes do primeiro frame
    void Start()
    {
        // Chama MostrarGameOver() quando a cena "Game Over" é carregada
        MostrarGameOver(); 
    }

    public void MostrarGameOver()
    {
        gameOverScreen.SetActive(true);
        int index = Random.Range(0, mensagensLatinas.Length);
        gameOverMessage.text = mensagensLatinas[index];
    }

    public void VoltarAoMenu()
    {
        SceneManager.LoadScene("Main Menu"); // Verifique se o nome da cena está correto
    }
}