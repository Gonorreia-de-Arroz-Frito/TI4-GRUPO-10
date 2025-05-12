using UnityEngine;

public class Walk : MonoBehaviour
{
    public float speed = 5f;
    private Animator animator;
    private Rigidbody2D rb;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        rb.linearVelocity = movement.normalized * speed;

        // Ativa a animação de andar se estiver se movendo
        animator.SetBool("isWalking", movement != Vector2.zero);
    }
}
