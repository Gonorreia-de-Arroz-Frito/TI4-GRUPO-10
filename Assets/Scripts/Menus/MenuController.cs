using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Arraste o GameObject do SEU PAINEL de opções aqui no Inspector
    [SerializeField] private GameObject optionsPanel;
    // Arraste sua Ação "Esc" do Input System Asset aqui
    [SerializeField] private InputActionReference toggleMenuActionReference;

    private bool isPanelOpen = false;

    void Awake()
    {
        // Garante que o painel de opções comece desativado
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
            Debug.Log("Menu habilitado!");
        }
        else
        {
            Debug.LogError("ToggleMenuActionReference não foi atribuída ou a ação é nula no MenuController!");
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

    // Chamado quando a ação (Esc) é performada
    private void OnToggleMenuPerformed(InputAction.CallbackContext context)
    {
        ToggleOptionsMenu();
    }

    // Alterna a visibilidade do painel de opções
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
            Debug.LogError("O 'Options Panel' não foi atribuído no MenuController!");
        }
    }

    // Função para retornar ao menu principal (Cena de índice 0)
    public void ReturnToMainMenu()
    {
        // Time.timeScale = 1f; // Removido, pois não estamos mais alterando
        SceneManager.LoadScene(0);
        Debug.Log("Retornando ao Menu Principal (Cena 0)");
    }
}