using UnityEngine;

public class EnemyWalk : MonoBehaviour
{
    public float speed = 5f;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    //[SerializeField] Transform tf;
    //Vector2 prevPos = Vector2.zero;

    private void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        

        // Ativa a animação de andar se estiver se movendo
        animator.SetBool("isWalking", rb.linearVelocity != Vector2.zero);
        //animator.SetFloat("walkSpeed", movement.magnitude);
    }
}
