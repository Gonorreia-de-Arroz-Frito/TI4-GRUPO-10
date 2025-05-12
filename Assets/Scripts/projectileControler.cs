using UnityEngine;

public class projectileControler : MonoBehaviour
{

    [ReadOnlyAtribute][SerializeField] Collider2D damageColision;
    [ReadOnlyAtribute][SerializeField] Collider2D physicsColision;
    [ReadOnlyAtribute][SerializeField] Collider2D rayTracableColider;
    [SerializeField] float damage;
    [SerializeField] public float forceMagnitude = 10f;

    [Header(" Non projectiles properties ")]
    [SerializeField] float _nppLinearDamp = 2f;
    [SerializeField] float _nppAngularDamp = 10f;
    [SerializeField] float _nppMass = 10f;

    [Header(" projectiles properties ")]
    [SerializeField] float linearDamp = 0.1f;
    [SerializeField] float angularDamp = 20f;
    [SerializeField] float mass = 50f;

    [Header(" air time properties ")]
    [SerializeField] public float airTime = 0.5f;
    [ReadOnlyAtribute][SerializeField] float airTimeCounter = 0f;

    void Start()
    {
        damageColision = GetComponent<BoxCollider2D>();
        physicsColision = transform.parent.GetComponent<BoxCollider2D>();
        rayTracableColider = transform.parent.GetComponentInChildren<GrabableObject>().rayTracableColider;
    }

    // Update is called once per frame
    void OnCollisionEnter2D(Collision2D collision)
    {
        atackGameObject(collision.gameObject);

        //GetComponentInParent<Rigidbody2D>().linearVelocity *= -0.5f;
    }

    void atackGameObject(GameObject target)
    {
        Atributes atributes = target.GetComponent<Atributes>();
        if (atributes != null)
        {
            atributes.genericHeathEffect(-damage);
            damageColision.enabled = false;
            disableProjectile();
        }
    }

    public void disableProjectile()
    {
        Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();

        rb.linearDamping = _nppLinearDamp;
        rb.angularDamping = _nppAngularDamp;
        rb.mass = _nppMass;

        damageColision.enabled = false;
        physicsColision.enabled = true;
        rayTracableColider.enabled = true;
    }

    public bool enableProjectile(bool enemy)
    {
        Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();

        rb.linearDamping = linearDamp;
        rb.angularDamping = angularDamp;
        rb.mass = mass;

        if (enemy)
        {
            damageColision.gameObject.layer = LayerMask.NameToLayer("EnemyAtack");
        }
        else
        {
            damageColision.gameObject.layer = LayerMask.NameToLayer("PlayerAtack");
        }

        damageColision.enabled = true;
        physicsColision.enabled = false;
        rayTracableColider.enabled = false;

        return !damageColision.IsTouchingLayers(LayerMask.GetMask("wall"));
    }

    public void timerStart()
    {
        airTimeCounter = airTime;
    }

    public void timerStop()
    {
        airTimeCounter = 0;
    }

    public void timerReset()
    {
        airTimeCounter = airTime;
    }

    void timerTick()
    {
        if (airTimeCounter > 0)
        {
            airTimeCounter -= Time.deltaTime;
            if (airTimeCounter <= 0)
            {
                airTimeCounter = 0;
                disableProjectile();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        timerTick();

    }
}
