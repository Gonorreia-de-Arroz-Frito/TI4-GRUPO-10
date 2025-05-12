using UnityEngine;

public class GrabableObject : MonoBehaviour
{

    [SerializeField] public Transform parentTransform;
    [SerializeField] public BoxCollider2D rayTracableColider;
    [SerializeField] public projectileControler projectileControler;
    [SerializeField] public Vector3 grabOffset = new Vector3(0, 0, 0);

    public void grab()
    {

    }

    public void shoot(Vector2 direction, bool enemy)
    {
        Debug.Log("attempted shooting \"" + gameObject.name + "\" in direction (" + direction + ") as: " + (enemy ? "enemy" : "player"));
        if (projectileControler != null)
        {
            projectileControler.enableProjectile(enemy);
            projectileControler.timerStart();
            Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();

            rb.AddTorque(Random.Range(-20f, 20f), ForceMode2D.Impulse);
            rb.AddForce(direction * projectileControler.forceMagnitude, ForceMode2D.Impulse);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (parentTransform == null)
            parentTransform = GetComponent<Transform>().parent.GetComponent<Transform>();
        if (rayTracableColider == null)
            rayTracableColider = GetComponent<BoxCollider2D>();
        if (projectileControler == null)
            projectileControler = GetComponent<Transform>().parent.GetComponentInChildren<projectileControler>();
    }

}
