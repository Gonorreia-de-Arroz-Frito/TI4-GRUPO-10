using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// Implementação concreta de um ataque tipo "Lunge" (investida).
/// O inimigo avança rapidamente em direção ao alvo após uma breve pausa.
/// </summary>
public class LungeAttack : EnemyAttackBase
{
    public UnityEvent OnAttackSequenceStart;
    [SerializeField] EnemyMovement enemyMovement;
    [Header("CONFIGURAÇÕES DO LUNGE")]
    [Tooltip("Pausa em segundos antes de iniciar a investida.")]
    [SerializeField] private float attackPreparationTime = 0.5f;
    [Tooltip("Força do impulso aplicado durante a investida.")]
    [SerializeField] private float lungeForce = 100f;
    [Tooltip("Distância máxima do alvo para iniciar o ataque.")]
    [SerializeField] private float stopDistance = 3f;

    // --- CONTROLE DE ESTADO ---
    private bool isPreparingAttack = false;

    /// <summary>
    /// Implementação do PerformAttack: dispara a animação e aplica a força da investida.
    /// Esta função não muda, pois a pausa acontece ANTES dela ser chamada.
    /// </summary>
    protected override void PerformAttack()
    {
        // Dispara a animação de ataque.
        animator.SetTrigger("Attack");

        if (playerTarget != null)
        {
            Vector2 directionToTarget = ((Vector2)playerTarget.position - rb.position).normalized;
            rb.linearVelocity = Vector2.zero; // Garante um impulso consistente resetando a velocidade atual.
            rb.AddForce(directionToTarget * lungeForce, ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// Sobrescreve a condição de ataque: verifica se o jogador está dentro do alcance para a investida.
    /// </summary>
    protected override void AttackCondition()
    {
        // Se não houver alvo ou se já estiver preparando um ataque, não faz nada.
        if (playerTarget == null || isPreparingAttack) return;

        float distanceToTarget = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToTarget <= stopDistance)
        {
            // Em vez de atacar diretamente, inicia a corrotina de preparação.
            StartCoroutine(PrepareAndLungeCoroutine());
        }
    }

    /// <summary>
    /// Corrotina que gerencia a pausa antes do ataque.
    /// </summary>
    private IEnumerator PrepareAndLungeCoroutine()
    {

        // 1. Entra no estado de "preparação".
        isPreparingAttack = true;
        OnAttackSequenceStart.Invoke();

        

        // 2. Aguarda o tempo definido.
        yield return new WaitForSeconds(attackPreparationTime);

        // 3. Após a espera, executa o ataque chamando o método da classe base.
        // Adicionamos uma checagem extra: se o jogador saiu do alcance durante a preparação, o ataque é cancelado.
        float distanceToTarget = Vector2.Distance(transform.position, playerTarget.position);
        if (distanceToTarget <= stopDistance)
        {
            InitiateAttack();
        }
        else
        {
            AbortAttackSequence();
        }

        // 4. Sai do estado de "preparação", permitindo que uma nova verificação comece.
        isPreparingAttack = false;
    }
}