using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class HitBox : MonoBehaviour
{
    [ReadOnlyAtribute][SerializeField] bool clearAfterAtack = false;
    [SerializeField] float damage = 10;
    [SerializeField] float knockback = 1;
    [SerializeField] float defaultAtackTime = 1f;
    [SerializeField] int atackMode;
    [SerializeField] Vector2 boxSize;
    [SerializeField] Vector2 boxPos;
    [SerializeField] LayerMask layerMask;
    [SerializeField] InputActionReference atackInput;
    [SerializeField] Animator animator;
    [SerializeField] movement movement;

    [Header("Debug")]
    [SerializeField] bool showInEditor = true;
    List<Collider2D> collideList = new List<Collider2D>();
    [ReadOnlyAtribute][SerializeField] float atackTime;
    [ReadOnlyAtribute][SerializeField] float counter = 0;


    public void atack(float time, float mode)
    {
        if (counter > 0)
            return;

        switch (mode)
        {
            case 0:
                kick();
                animator.SetTrigger("kick");
                break;

            default:
                kick();
                animator.SetTrigger("kick");
                break;
        }
        clearAfterAtack = true;
        atackTime = time;
        counter = time;

        movement.feetToTorso = true;
    }

    public void kick()
    {

    }

    private void Update()
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
                    collideList.Add(hit);
                    Vector2 direction = (Vector2)hit.transform.position - (Vector2)transform.position;
                    hit.GetComponent<Atributes>().hurt(damage, direction*knockback);
                }
            }
        }
        else if (clearAfterAtack)
        {
            collideList.Clear();
            clearAfterAtack = false;
            movement.feetToTorso = false;
        }
    }

    void OnEnable()
    {
        atackInput.action.started += defaultAtack;
    }
    void OnDisable()
    {
        atackInput.action.started -= defaultAtack;
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
