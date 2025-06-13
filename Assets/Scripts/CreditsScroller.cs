using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para gerenciar cenas

public class CreditsScroller : MonoBehaviour
{
    [Tooltip("O objeto de conteúdo que contém todos os textos dos créditos.")]
    [SerializeField]
    private RectTransform creditsContent;

    [Tooltip("A velocidade com que os créditos rolam para cima.")]
    [SerializeField]
    private float scrollSpeed = 70f;

    private Vector2 initialPosition;

    void Start()
    {
        // Salva a posição inicial para poder reiniciar os créditos
        if (creditsContent != null)
        {
            initialPosition = creditsContent.anchoredPosition;
        }
    }

    void Update()
    {
        if (creditsContent == null) return;

        // Move o conteúdo dos créditos para cima
        creditsContent.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

        // Verifica se os créditos já saíram completamente da tela
        // A condição é: a base do conteúdo (anchoredPosition.y) passou do topo da tela (Screen.height)?
        if (creditsContent.anchoredPosition.y > Screen.height + creditsContent.rect.height)
        {
            // Quando termina, volta para o início para um loop infinito
            // Ou você pode carregar o menu principal aqui
            ResetCredits();
            // Exemplo para voltar ao menu: SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Reinicia a posição dos créditos para o início.
    /// </summary>
    public void ResetCredits()
    {
        creditsContent.anchoredPosition = initialPosition;
    }

    /// <summary>
    /// Função pública para ser chamada por um botão para voltar ao menu.
    /// </summary>
    public void BackToMainMenu()
    {
        // Certifique-se de que o nome da sua cena do menu principal está correto
        Debug.Log("Voltando para o Menu Principal...");
        SceneManager.LoadScene("MainMenu"); 
    }
}