using System.Collections;
using System.Collections.Generic;
using System; 
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Classe base abstrata para todos os comportamentos de ataque de inimigos.
/// Gerencia cooldown, dura��o do ataque, detec��o de hitbox e aplica��o de dano.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public abstract class EnemyAttackBase : MonoBehaviour
{
    public UnityEvent OnAttackSequenceEnd;
    [Header("CONFIGURA��ES GERAIS DE ATAQUE")]
    [Tooltip("Dano infligido ao alvo.")]
    [SerializeField] protected float damage = 10f;
    [Tooltip("For�a do knockback aplicado ao alvo.")]
    [SerializeField] protected float knockback = 1f;
    [Tooltip("Dura��o total em que a hitbox permanece ativa.")]
    [SerializeField] protected float attackDuration = 1f;
    [Tooltip("Tempo de espera m�nimo entre ataques.")]
    [SerializeField] protected float cooldownTime = 1f;
    [Tooltip("O alvo do ataque, geralmente o jogador.")]
    [SerializeField] protected Transform playerTarget;
    [Tooltip("Layers que ser�o consideradas como alvos v�lidos.")]
    [SerializeField] protected LayerMask targetLayerMask;

    [Header("HITBOX")]
    [Tooltip("Dimens�es da �rea de detec��o de dano.")]
    [SerializeField] protected Vector2 hitboxSize = Vector2.one;
    [Tooltip("Deslocamento da hitbox em rela��o ao piv� do GameObject.")]
    [SerializeField] protected Vector2 hitboxOffset = Vector2.zero;

    [Header("COMPONENTES")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected Rigidbody2D rb;

    // --- CONTROLE DE ESTADO ---
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private float cooldownTimer = 0f;
    private List<Collider2D> alreadyHitTargets = new List<Collider2D>();

    /// <summary>
    /// Cache de refer�ncias de componentes para evitar chamadas repetidas de GetComponent().
    /// </summary>
    protected virtual void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    // ##################################################################
    // #                 A L�GICA DE UPDATE FOI ALTERADA                #
    // ##################################################################
    /// <summary>
    /// Gerencia os timers e os estados de ataque/cooldown.
    /// </summary>
    protected virtual void Update()
    {
        // Decrementa os timers com base no tempo do jogo.
        cooldownTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;

        // Se o inimigo est� no meio de um ataque...
        if (isAttacking)
        {
            // ...verifica continuamente por alvos na hitbox.
            DetectAndDamage();

            // Finaliza o ataque quando a dura��o termina.
            if (attackTimer <= 0f)
            {
                FinishAttack();
            }
        }
        // Se n�o estiver atacando e nem em cooldown, verifica a condi��o para um novo ataque.
        else if (cooldownTimer <= 0f)
        {
            AttackCondition();
        }
    }

    /// <summary>
    /// Inicia a sequ�ncia de ataque. Define os timers e o estado.
    /// </summary>
    public void InitiateAttack()
    {
        if (cooldownTimer > 0 || isAttacking) return;

        isAttacking = true;
        attackTimer = attackDuration;
        cooldownTimer = cooldownTime;

        // Chama a implementa��o espec�fica do ataque da classe filha.
        PerformAttack();
    }

    /// <summary>
    /// M�todo abstrato para a execu��o do ataque.
    /// A ser implementado pela classe filha com a l�gica espec�fica (ex: anima��o, movimento).
    /// </summary>
    protected abstract void PerformAttack();

    /// <summary>
    /// M�todo virtual que define a condi��o para iniciar um ataque.
    /// Pode ser sobrescrito para diferentes gatilhos (dist�ncia, linha de vis�o, etc.).
    /// </summary>
    protected virtual void AttackCondition() { }

    /// <summary>
    /// Detecta e aplica dano aos alvos dentro da hitbox.
    /// A lista 'alreadyHitTargets' previne que o dano seja aplicado m�ltiplas vezes no mesmo ataque.
    /// </summary>
    public virtual void DetectAndDamage()
    {
        Vector2 center = (Vector2)transform.position + (Vector2)(Quaternion.Euler(0, 0, transform.eulerAngles.z) * hitboxOffset);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitboxSize, transform.eulerAngles.z, targetLayerMask);

        foreach (Collider2D hit in hits)
        {
            // Garante que cada alvo seja atingido apenas uma vez por ataque.
            if (!alreadyHitTargets.Contains(hit))
            {
                // Tenta obter um componente "damageable" no alvo.
                if (hit.TryGetComponent<Atributes>(out Atributes targetAttributes))
                {
                    Vector2 direction = ((Vector2)hit.transform.position - rb.position).normalized;
                    targetAttributes.hurt(damage, direction * knockback);
                    alreadyHitTargets.Add(hit);
                }
            }
        }
    }

    /// <summary>
    /// Reseta o estado de ataque e limpa la lista de alvos atingidos.
    /// </summary>
    private void FinishAttack()
    {
        isAttacking = false;
        alreadyHitTargets.Clear();

        
        OnAttackSequenceEnd.Invoke();
    }
    public void AbortAttackSequence()
    {
        isAttacking = false;
        OnAttackSequenceEnd.Invoke();
    }



#if UNITY_EDITOR
    [Header("DEBUG")]
    [SerializeField] private bool showGizmos = true;

    /// <summary>
    /// Desenha a hitbox no Editor para facilitar o debugging visual.
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = isAttacking ? Color.red : Color.yellow;

        Vector2 center = (Vector2)transform.position + (Vector2)(Quaternion.Euler(0, 0, transform.eulerAngles.z) * hitboxOffset);

        // Aplica rota��o e posi��o ao Gizmo para corresponder � orienta��o do objeto.
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, transform.eulerAngles.z), Vector3.one);

        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
        Gizmos.DrawCube(Vector3.zero, hitboxSize);
    }
#endif
}