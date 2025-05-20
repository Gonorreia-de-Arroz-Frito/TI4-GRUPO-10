using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

public class movement : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform tf;
    [SerializeField] Camera mainCamera;

    [Header("Movement")]
    [SerializeField] float acceleration = 20F;
    [SerializeField] float topSpeed = 10F;
    [SerializeField] float dragCoefficient;
    [SerializeField] float stopedDragCoefficient;

    [Header("Rotation")]
    [SerializeField] public bool lookAtMouse = true;
    [SerializeField] float rotationAccel = 90F;
    [SerializeField] float lookFOV = 45F;
    [SerializeField] float lookDebugRange = 10F;

    [Header("Debug")]
    [SerializeField] public bool gizmosToggle = true;
    [ShowIf(ActionOnConditionFail.JUST_DISABLE, ConditionOperator.AND, nameof(gizmosToggle))][SerializeField] bool viewAngleGizmos = true;
    [ShowIf(ActionOnConditionFail.JUST_DISABLE, ConditionOperator.AND, nameof(gizmosToggle))][SerializeField] bool velocityGizmos = true;
    [ShowIf(ActionOnConditionFail.JUST_DISABLE, ConditionOperator.AND, nameof(gizmosToggle))][SerializeField] bool wishDirectionGizmos = true;

    [Header("Show Only")]
    [ReadOnlyAtribute][SerializeField] Vector2 wishDir;
    [ReadOnlyAtribute][SerializeField] float dotVel;
    [ReadOnlyAtribute][SerializeField] Vector2 lookDir;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (tf == null)
            tf = GetComponent<Transform>();
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {


    }

    void FixedUpdate()
    {
        wishDirCalculation();
        movementCalculations();
        dragCalculations();

        lookDirection();
    }

    void lookDirection()
    {

        if (lookAtMouse)
        {
            lookDir = mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        }
        else if (wishDir != Vector2.zero)
            lookDir = wishDir;
        if (lookDir != Vector2.zero)
        {
            Quaternion target = Quaternion.LookRotation(tf.forward, lookDir);
            Quaternion rotation = Quaternion.RotateTowards(tf.rotation, target, rotationAccel * Time.deltaTime * 100);

            rb.MoveRotation(rotation);
        }
    }

    // Basic Speed calculation using quake as inspiration
    void movementCalculations()
    {
        float accel = acceleration * Time.deltaTime * 1000;

        /*
        dotVel = Vector2.Dot(wishDir, rb.linearVelocity);
        float vel = topSpeed - dotVel;
        if (vel <= 0) return;

        accel = math.min(vel, accel);
        */

        rb.AddForce(wishDir * accel);

        if (rb.linearVelocity.magnitude > topSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * topSpeed; // Limit the speed to the top speed
        }
    }

    // Drag Calculations
    void dragCalculations()
    {
        if (wishDir != Vector2.zero)
            rb.AddForce(-1 * (rb.linearVelocity / 2) * dragCoefficient * Time.deltaTime * 1000);
        else
            rb.AddForce(-1 * (rb.linearVelocity / 2) * stopedDragCoefficient * Time.deltaTime * 1000);
    }

    // Wish direction
    void wishDirCalculation()
    {
        wishDir.x = Input.GetAxisRaw("Horizontal");
        wishDir.y = Input.GetAxisRaw("Vertical");
        wishDir = wishDir.normalized;
    }

    void OnDrawGizmos()
    {
        if (gizmosToggle)
        {
            if (wishDirectionGizmos)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(tf.position, new Vector3(tf.position.x + wishDir.x, tf.position.y + wishDir.y, tf.position.z));
            }

            if (velocityGizmos)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(tf.position, new Vector3(tf.position.x + rb.linearVelocity.x, tf.position.y + rb.linearVelocity.y, tf.position.z));
            }

            if (viewAngleGizmos)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(tf.position, tf.position + ((Quaternion.AngleAxis(lookFOV, new Vector3(0f, 0f, 1)) * tf.rotation * new Vector3(0f, 1f, 0)).normalized * lookDebugRange));
                Gizmos.DrawLine(tf.position, tf.position + ((Quaternion.AngleAxis(-lookFOV, new Vector3(0f, 0f, 1)) * tf.rotation * new Vector3(0f, 1f, 0)).normalized * lookDebugRange));
            }
        }
    }
}
