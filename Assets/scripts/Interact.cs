using System;
using Mono.Cecil;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Interact : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] movement playerMovement;
    [SerializeField] RelativeJoint2D relativeJoint;
    [SerializeField] Transform tf;
    [SerializeField] Animator animator;
    [ReadOnlyAtribute][SerializeField] GameObject target;
    [ReadOnlyAtribute][SerializeField] GrabableObject grabableObject;
    [ReadOnlyAtribute][SerializeField] bool grabing;
    [ReadOnlyAtribute][SerializeField] float magnitude;
    [ReadOnlyAtribute][SerializeField] float push;
    [ReadOnlyAtribute][SerializeField] Vector3 pushDir;
    [ReadOnlyAtribute][SerializeField] Vector3 lookDir;

    [Header("Interaction options")]
    [SerializeField] Vector3 objectOffset;
    [SerializeField] float range = 10f;
    [SerializeField] float maxPush = 10f;
    [SerializeField] bool OverridePlayerRotation = true;
    [SerializeField] float releaseImpulse = 2f;
    [SerializeField] LayerMask layermask;
    [SerializeField] LayerMask confirmLayermask;

    [Header("Trow Animation")]
    [SerializeField] float trowMaxChargeSecounds = 1.5f;



    [Header("Config")]
    [SerializeField] LayerMask trowHelperLayer;
    [SerializeField] LayerMask wallMask;
    [SerializeField] bool drawRangeGizmo = true;
    [SerializeField] float raycastOffset = 0.5f;
    [SerializeField] InputActionReference grab;
    [SerializeField] InputActionReference shoot;
    [SerializeField] InputActionReference damageMyself;
    [SerializeField] Camera mainCamera;


    Vector3 objRotOffset;
    float objectMass;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if (tf == null)
        {
            tf = GetComponent<Transform>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        lookDir = mainCamera.ScreenToWorldPoint(Input.mousePosition) - tf.position;
        lookDir.z = 0;
        lookDir.Normalize();

        if (grabing)
        {

        }
        else
            raycast();

        if (OverridePlayerRotation)
        {
            if (target != null)
            {
                playerMovement.lookAtMouse = true;
            }
            else
            {
                playerMovement.lookAtMouse = false;
            }
        }

    }

    void OnEnable()
    {
        grab.action.started += GrabAttempt;
        shoot.action.performed += shootAttempt;
        damageMyself.action.performed += (ctx) => { GetComponentInParent<Atributes>().hurt(1); };
    }

    void OnDisable()
    {
        grab.action.started -= GrabAttempt;
        shoot.action.performed -= shootAttempt;
        damageMyself.action.performed -= (ctx) => { GetComponentInParent<Atributes>().hurt(1); };
    }


    void shootAttempt(InputAction.CallbackContext obj)
    {

        if (grabableObject != null)
        {
            GameObject proj = grabableObject.gameObject;

            releaseTarget();

            proj.GetComponent<GrabableObject>().shoot(lookDir, false);
        }
    }


    private void GrabAttempt(InputAction.CallbackContext obj)
    {
        if (grabableObject != null)
            releaseTarget();
        else if (target != null)
            grabableObject = target.GetComponent<GrabableObject>();

        if (grabableObject != null)
        {
            grabTarget();
        }
    }


    Vector2 v3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.y);
    }
    void grabTarget()
    {
        if (grabableObject != null)
        {
            grabing = true;
            target.GetComponentInChildren<BoxCollider2D>().enabled = false;
            if (OverridePlayerRotation)
                playerMovement.lookAtMouse = true;

            relativeJoint.linearOffset = objectOffset + grabableObject.grabOffset;
            //relativeJoint. = 0.005f;
            // Connect to object
            relativeJoint.connectedBody = target.GetComponentInParent<Rigidbody2D>();
            relativeJoint.enabled = true;
            objectMass = target.GetComponentInParent<Rigidbody2D>().mass;
            target.GetComponentInParent<Rigidbody2D>().mass = 0.1f;

            // animation
            animator.SetBool("grabing", true);

        }
    }

    void releaseTarget()
    {
        // Disconect from object
        relativeJoint.connectedBody = null;
        relativeJoint.enabled = false;
        target.GetComponentInParent<Rigidbody2D>().mass = objectMass;

        target.GetComponentInChildren<BoxCollider2D>().enabled = true;
        grabing = false;
        target = null;
        GameObject proj = grabableObject.gameObject;
        grabableObject = null;
        if (OverridePlayerRotation)
            playerMovement.lookAtMouse = false;

        // animation
        animator.SetBool("grabing", false);

        proj.transform.parent.GetComponent<Rigidbody2D>().AddForce(lookDir.normalized * releaseImpulse * proj.transform.parent.GetComponent<Rigidbody2D>().mass, ForceMode2D.Impulse);
    }

    private void raycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + (lookDir.normalized * raycastOffset), lookDir, range, layermask);
        if (hit.collider != null)
        {
            target = hit.collider.gameObject;
        }
        else
        {
            target = null;
        }
    }

    void OnDrawGizmos()
    {
        if (drawRangeGizmo)
        {
            Gizmos.color = Color.red;

            if (!grabing)
            {
                Gizmos.DrawLine(tf.position + (lookDir * raycastOffset), tf.position + (lookDir * raycastOffset) + (lookDir * range));
            }
            else
            {
                Vector3 grabCube = new Vector3(0.1f, 0.1f, 0);
                Vector3 grabCenter = tf.position + (tf.rotation * objectOffset);
                Vector3 objectcube = new Vector3(0.02f, 0.02f, 0);
                Vector3 objectPivot = tf.position + objRotOffset;

                Gizmos.DrawLine(grabCenter + new Vector3(grabCube.x, grabCube.y, grabCube.z), grabCenter + new Vector3(-grabCube.x, grabCube.y, grabCube.z));
                Gizmos.DrawLine(grabCenter + new Vector3(-grabCube.x, grabCube.y, grabCube.z), grabCenter + new Vector3(-grabCube.x, -grabCube.y, grabCube.z));
                Gizmos.DrawLine(grabCenter + new Vector3(-grabCube.x, -grabCube.y, grabCube.z), grabCenter + new Vector3(grabCube.x, -grabCube.y, grabCube.z));
                Gizmos.DrawLine(grabCenter + new Vector3(grabCube.x, -grabCube.y, grabCube.z), grabCenter + new Vector3(grabCube.x, grabCube.y, grabCube.z));

                Gizmos.color = Color.green;
                Gizmos.DrawLine(objectPivot + new Vector3(objectcube.x, objectcube.y, objectcube.z), objectPivot + new Vector3(-objectcube.x, objectcube.y, objectcube.z));
                Gizmos.DrawLine(objectPivot + new Vector3(-objectcube.x, objectcube.y, objectcube.z), objectPivot + new Vector3(-objectcube.x, -objectcube.y, objectcube.z));
                Gizmos.DrawLine(objectPivot + new Vector3(-objectcube.x, -objectcube.y, objectcube.z), objectPivot + new Vector3(objectcube.x, -objectcube.y, objectcube.z));
                Gizmos.DrawLine(objectPivot + new Vector3(objectcube.x, -objectcube.y, objectcube.z), objectPivot + new Vector3(objectcube.x, objectcube.y, objectcube.z));
                //Gizmos.DrawCube(tf.position + objRotOffset, new Vector3(1, 1, 0));
            }
        }

    }
}
