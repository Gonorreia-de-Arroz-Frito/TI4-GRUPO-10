using UnityEngine;

/// <summary>
/// Implementa��o concreta de um ataque tipo "Lunge" (investida).
/// O inimigo avan�a rapidamente em dire��o ao alvo ao atacar.
/// </summary>
public class LungeAttack : EnemyAttackBase
{
    [Header("CONFIGURA��ES DO LUNGE")]
    [Tooltip("For�a do impulso aplicado durante a investida.")]
    [SerializeField] private float lungeForce = 100f;
    [Tooltip("Dist�ncia m�xima do alvo para iniciar o ataque.")]
    [SerializeField] private float stopDistance = 3f;

    /// <summary>
    /// Implementa��o do PerformAttack: dispara a anima��o e aplica a for�a da investida.
    /// </summary>
    protected override void PerformAttack()
    {
        // Dispara a anima��o. O dano ser� aplicado pelo Animation Event chamando DetectAndDamage().
        animator.SetTrigger("Attack");

        if (playerTarget != null)
        {
            Vector2 directionToTarget = ((Vector2)playerTarget.position - rb.position).normalized;
            rb.linearVelocity = Vector2.zero; // Garante um impulso consistente resetando a velocidade atual.
            rb.AddForce(directionToTarget * lungeForce, ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// Sobrescreve a condi��o de ataque: verifica se o jogador est� dentro do alcance para a investida.
    /// </summary>
    protected override void AttackCondition()
    {
        if (playerTarget == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToTarget <= stopDistance)
        {
            // Chama o m�todo da classe base para come�ar o processo de ataque.
            InitiateAttack();
        }
    }
}