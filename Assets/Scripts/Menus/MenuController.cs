using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Arraste o GameObject do SEU PAINEL de op��es aqui no Inspector
    [SerializeField] private GameObject optionsPanel;
    // Arraste sua A��o "Esc" do Input System Asset aqui
    [SerializeField] private InputActionReference toggleMenuActionReference;

    private bool isPanelOpen = false;

    void Awake()
    {
        // Garante que o painel de op��es comece desativado
        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            optionsPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (toggleMenuActionReference != null && toggleMenuActionReference.action != null)
        {
            toggleMenuActionReference.action.performed += OnToggleMenuPerformed;
            toggleMenuActionReference.action.Enable();
        }
        else
        {
            Debug.LogError("ToggleMenuActionReference n�o foi atribu�da ou a a��o � nula no MenuController!");
        }
    }

    void OnDisable()
    {
        if (toggleMenuActionReference != null && toggleMenuActionReference.action != null)
        {
            toggleMenuActionReference.action.performed -= OnToggleMenuPerformed;
            toggleMenuActionReference.action.Disable();
            Debug.Log("Menu desabilitado!");
        }
    }

    // Chamado quando a a��o (Esc) � performada
    private void OnToggleMenuPerformed(InputAction.CallbackContext context)
    {
        ToggleOptionsMenu();
    }

    // Alterna a visibilidade do painel de op��es
    public void ToggleOptionsMenu()
    {
        isPanelOpen = !isPanelOpen;
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(isPanelOpen);
        }
        else
        {
            // Mensagem de erro corrigida para referenciar 'optionsPanel'
            Debug.LogError("O 'Options Panel' n�o foi atribu�do no MenuController!");
        }
    }

    // Fun��o para retornar ao menu principal (Cena de �ndice 0)
    public void ReturnToMainMenu()
    {
        // Time.timeScale = 1f; // Removido, pois n�o estamos mais alterando
        SceneManager.LoadScene(0);
        Debug.Log("Retornando ao Menu Principal (Cena 0)");
    }
}