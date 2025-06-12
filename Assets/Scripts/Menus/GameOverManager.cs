using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverScreen;
    public TextMeshProUGUI gameOverMessage;

    private string[] mensagensLatinas = {
        "Corpus tuum cecidit, anima tua hic remanet.",
        "In tenebris, solus remanebis.",
        "Non est pax pro te.",
        "Oratio tua non audita est.",
        "Finis est initium doloris."
    };
    
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
