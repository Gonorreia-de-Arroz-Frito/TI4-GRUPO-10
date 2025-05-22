using UnityEngine;

public class Walk : MonoBehaviour
{
    public float speed = 5f;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;

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
        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        rb.linearVelocity = movement.normalized * speed;

        // Ativa a animação de andar se estiver se movendo
        animator.SetBool("isWalking", movement != Vector2.zero);
        //animator.SetFloat("walkSpeed", movement.magnitude);
    }
}
