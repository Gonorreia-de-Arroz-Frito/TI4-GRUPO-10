using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class movement : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Transform tf;
    [SerializeField] Camera mainCamera;

    [Header("Movement")]
    [SerializeField] float acceleration = 20F;
    [SerializeField] float topSpeed = 10F;
    [SerializeField] float dragCoefficient;
    [SerializeField] float stopedDragCoefficient;

    [Header("Rotation")]
    [SerializeField] public bool feetToTorso = true;
    [SerializeField] float rotationAccel = 90F;
    [SerializeField] float lookFOV = 45F;
    [SerializeField] float lookDebugRange = 10F;
    [SerializeField] Transform[] torsoRot;
    [SerializeField] Transform[] feetRot;

    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepSounds;

    // --- KONAMI CODE: VARIÁVEIS ---
    [Header("Konami Code")]
    [SerializeField] private AudioClip[] konamiFootstepSounds; // Sons de passo após o código
    [SerializeField] private AudioClip konamiSuccessSound;   // Som de sucesso ao ativar
    
    private KeyCode[] konamiCodeSequence;
    private int konamiIndex = 0;
    private bool konamiCodeActivated = false;
    // -----------------------------

    [Header("Debug")]
    [SerializeField] public bool gizmosToggle = true;
    [ShowIf(ActionOnConditionFail.JUST_DISABLE, ConditionOperator.AND, nameof(gizmosToggle))][SerializeField] bool viewAngleGizmos = true;
    [ShowIf(ActionOnConditionFail.JUST_DISABLE, ConditionOperator.AND, nameof(gizmosToggle))][SerializeField] bool velocityGizmos = true;
    [ShowIf(ActionOnConditionFail.JUST_DISABLE, ConditionOperator.AND, nameof(gizmosToggle))][SerializeField] bool wishDirectionGizmos = true;

    [Header("Show Only")]
    [ReadOnlyAtribute][SerializeField] Vector2 wishDir;
    [ReadOnlyAtribute][SerializeField] float dotVel;
    [ReadOnlyAtribute][SerializeField] Vector2 TorsoLookDir;
    [ReadOnlyAtribute][SerializeField] Vector2 FeetLookDir;
    [ReadOnlyAtribute][SerializeField] bool invertRunAnimation = false;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (tf == null) tf = GetComponent<Transform>();
        if (mainCamera == null) mainCamera = Camera.main;
        if (animator == null) animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        // --- KONAMI CODE: INICIALIZAÇÃO ---
        // Cima, Cima, Baixo, Baixo, Esquerda, Direita, Esquerda, Direita, B, A
        konamiCodeSequence = new KeyCode[] {
            KeyCode.W, KeyCode.W, KeyCode.S, KeyCode.S,
            KeyCode.A, KeyCode.D, KeyCode.A, KeyCode.D,
            KeyCode.B, KeyCode.A
        };
        // ----------------------------------
    }

    void Update()
    {
        // --- KONAMI CODE: VERIFICAÇÃO ---
        // O método Update é o lugar ideal para verificar inputs de tecla a cada frame.
        // Só executa se o código ainda não foi ativado.
        if (!konamiCodeActivated)
        {
            CheckKonamiCode();
        }
        // ---------------------------------
    }

    void FixedUpdate()
    {
        wishDirCalculation();
        movementCalculations();
        dragCalculations();
        lookDirection();
        UpdateAnimator();
    }
    
    // --- KONAMI CODE: LÓGICA DE VERIFICAÇÃO ---
    private void CheckKonamiCode()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(konamiCodeSequence[konamiIndex]))
            {
                konamiIndex++; // Avança na sequência se acertou a tecla
            }
            else
            {
                konamiIndex = 0; // Reinicia se errou a tecla
            }

            if (konamiIndex == konamiCodeSequence.Length)
            {
                ActivateKonamiCode();
            }
        }
    }
    
    private void ActivateKonamiCode()
    {
        Debug.Log("KONAMI CODE ATIVADO! PASSOS SECRETOS LIBERADOS!");
        konamiCodeActivated = true;
        konamiIndex = 0; // Reinicia para não ficar preso

        if (konamiSuccessSound != null)
        {
            audioSource.PlayOneShot(konamiSuccessSound);
        }
    }
    // ----------------------------------------

    void UpdateAnimator()
    {
        animator.SetBool("isWalking", wishDir != Vector2.zero);
        animator.SetBool("isWalkingBackward", invertRunAnimation);
    }

    // --- FUNÇÃO DE SOM MODIFICADA ---
    public void PlayFootstepSound()
    {
        // Decide qual array de sons usar baseado na flag 'konamiCodeActivated'
        AudioClip[] soundsToUse = konamiCodeActivated ? konamiFootstepSounds : footstepSounds;
        
        if (soundsToUse != null && soundsToUse.Length > 0)
        {
            int index = UnityEngine.Random.Range(0, soundsToUse.Length);
            audioSource.PlayOneShot(soundsToUse[index]);
        }
    }
    // -----------------------------

    #region Movimentação e Rotação (Sem alterações)
    void lookDirection()
    {
        TorsoLookDir = mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        if (wishDir != Vector2.zero && !feetToTorso)
            FeetLookDir = wishDir;
        else
            FeetLookDir = TorsoLookDir;

        float angleBetween = Vector2.Angle(TorsoLookDir, FeetLookDir);
        if (angleBetween > 90f)
        {
            FeetLookDir *= -1;
            invertRunAnimation = true;
        }
        else
            invertRunAnimation = false;

        foreach (Transform tfm in torsoRot)
        {
            tfm.rotation = Quaternion.LookRotation(tfm.forward, TorsoLookDir);
        }
        foreach (Transform tfm in feetRot)
        {
            tfm.rotation = Quaternion.LookRotation(tfm.forward, FeetLookDir);
        }
    }

    void movementCalculations()
    {
        float accel = acceleration * Time.deltaTime * 1000;
        rb.AddForce(wishDir * accel);

        if (rb.linearVelocity.magnitude > topSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * topSpeed;
        }
    }

    void dragCalculations()
    {
        if (wishDir != Vector2.zero)
            rb.AddForce(-1 * (rb.linearVelocity / 2) * dragCoefficient * Time.deltaTime * 1000);
        else
            rb.AddForce(-1 * (rb.linearVelocity / 2) * stopedDragCoefficient * Time.deltaTime * 1000);
    }

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
    #endregion
}