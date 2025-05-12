using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI dicaText;
        public Image fadeImage; // A imagem preta para fade
        private int cenaParaCarregar;

    private static string[] dicas = new string[]
    {
        "Dica: Tecnoblade Never Dies!",
        "Dica: Inimigos fracos podem atacar em grupo!",
        "Dica: Explore as áreas secretas!",
        "Dica: Use o ambiente ao seu favor!"
    };

    private void Start()
    {
        cenaParaCarregar = int.Parse(PlayerPrefs.GetString("CenaParaCarregar"));
        
        int indiceAleatorio = Random.Range(0, dicas.Length);

        dicaText.text = dicas[indiceAleatorio];

        StartCoroutine(CarregarCena());
    }

    IEnumerator CarregarCena()
    {
        // Primeiro: Fade In
        yield return StartCoroutine(FadeIn());

        // Depois: Começa a carregar a cena desejada
        AsyncOperation operacao = SceneManager.LoadSceneAsync(cenaParaCarregar);
        operacao.allowSceneActivation = false; // Espera manualmente para ativar

        while (operacao.progress < 0.9f)
        {
            float progresso = Mathf.Clamp01(operacao.progress / 0.9f);
            progressBar.value = progresso;
            yield return null;
        }

        // Quando a cena terminar de carregar (90%), mostra a barra cheia
        progressBar.value = 1f;

        // Pequena espera para dar sensação de carregamento completo
        yield return new WaitForSeconds(0.5f);

        // Agora: Fade Out
        yield return StartCoroutine(FadeOut());

        // Ativa a cena carregada
        operacao.allowSceneActivation = true;
    }

    IEnumerator FadeIn()
    {
        float duracao = 1f;
        float tempo = 0f;

        Color cor = fadeImage.color;
        cor.a = 1f;
        fadeImage.color = cor;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            cor.a = 1f - (tempo / duracao);
            fadeImage.color = cor;
            yield return null;
        }

        // Garante que o Alpha fique 0 no final
        cor.a = 0f;
        fadeImage.color = cor;
    }

    IEnumerator FadeOut()
    {
        float duracao = 1f;
        float tempo = 0f;

        Color cor = fadeImage.color;
        cor.a = 0f;
        fadeImage.color = cor;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            cor.a = tempo / duracao;
            fadeImage.color = cor;
            yield return null;
        }

        // Garante que o Alpha fique 1 no final
        cor.a = 1f;
        fadeImage.color = cor;
    }
}
