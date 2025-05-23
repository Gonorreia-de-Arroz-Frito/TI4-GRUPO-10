using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EnemyAttack : MonoBehaviour
{
    [ReadOnlyAtribute][SerializeField] bool clearAfterAtack = false;
    [SerializeField] float damage = 10;
    [SerializeField] float knockback = 1;
    [SerializeField] float defaultAtackTime = 1f;
    [SerializeField] int atackMode;
    [SerializeField] Vector2 boxSize;
    [SerializeField] Vector2 boxPos;
    [SerializeField] LayerMask layerMask;
    [SerializeField] Animator animator;
    [SerializeField] Transform playerTarget;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float cooldownTime = 1f;



    [Header("Debug")]
    [SerializeField] bool showInEditor = true;
    List<Collider2D> collideList = new List<Collider2D>();
    [ReadOnlyAtribute][SerializeField] float atackTime;
    [ReadOnlyAtribute][SerializeField] float counter = 0;

    [SerializeField] float lungeForce = 100f;
    [ReadOnlyAtribute] float lungeDuration = 0.5f;
    [ReadOnlyAtribute][SerializeField] float cooldownCounter = 1f;
    [SerializeField] float stopDistance = 10f;

    private bool isCurrentlyLunging = false;


    public void atack(float time, float mode)
    {
        if (counter > 0)
            return;

        switch (mode)
        {
            case 0:
                lunge();
                animator.SetTrigger("Attack");
                break;

            default:
                lunge();
                animator.SetTrigger("Attack");
                break;
        }
        clearAfterAtack = true;
        atackTime = time;
        counter = time;

        
    }

    public void lunge()
    {

    }

    
    public void AttackCondition()
    {

        if (cooldownCounter <= 0)
        {
            
            float distanceToTarget = Vector2.Distance(transform.position, playerTarget.position);

            if (distanceToTarget < stopDistance)
            {
                atack(defaultAtackTime, atackMode);
                cooldownCounter = cooldownTime;
                isCurrentlyLunging = true;
                Vector2 directionToTarget = ((Vector2)playerTarget.position - rb.position).normalized;
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(directionToTarget * lungeForce, ForceMode2D.Impulse);

                Invoke(nameof(ResetLungeState), lungeDuration);

                Debug.Log(gameObject.name + " executou um ataque Lunge!");
            }

        }
        else
        {
            cooldownCounter -= Time.deltaTime;
        }

    }

    private void ResetLungeState()
    {
        isCurrentlyLunging = false;
        rb.linearVelocity = Vector2.zero;
    }


    private void Update()
    {
        attackController();
        if(playerTarget != null)
        {
            AttackCondition();
        }
    }

    private void attackController()
    {
        counter -= Time.deltaTime;
        if (counter > 0)
        {
            Vector2 center = (Vector2)transform.position + (Vector2)(Quaternion.Euler(0, 0, transform.eulerAngles.z) * boxPos);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, transform.eulerAngles.z, layerMask);

            foreach (Collider2D hit in hits)
            {
                if (!collideList.Contains(hit))
                {
                    Vector2 direction = (Vector2)hit.transform.position - (Vector2)transform.position;
                    collideList.Add(hit);
                    hit.GetComponent<Atributes>().hurt(damage, direction*knockback);
                }
            }
        }
        else if (clearAfterAtack)
        {
            collideList.Clear();
            clearAfterAtack = false;
            
        }
    }
    void defaultAtack(InputAction.CallbackContext obj)
    {
        atack(defaultAtackTime, atackMode);
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {

        if (!showInEditor)
            return;

        // Calculate the center position of the box in world space
        Vector2 center = (Vector2)transform.position + (Vector2)(Quaternion.Euler(0, 0, transform.eulerAngles.z) * boxPos);

        // Calculate the four corners of the box (quad) based on rotation
        Quaternion rot = Quaternion.Euler(0, 0, transform.eulerAngles.z);
        Vector2 halfSize = boxSize * 0.5f;

        Vector2 topLeft = center + (Vector2)(rot * new Vector2(-halfSize.x, halfSize.y));
        Vector2 topRight = center + (Vector2)(rot * new Vector2(halfSize.x, halfSize.y));
        Vector2 bottomRight = center + (Vector2)(rot * new Vector2(halfSize.x, -halfSize.y));
        Vector2 bottomLeft = center + (Vector2)(rot * new Vector2(-halfSize.x, -halfSize.y));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        if (!Application.isPlaying || counter > 0)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        }
        Gizmos.DrawCube(center, boxSize);
    }
#endif


}
