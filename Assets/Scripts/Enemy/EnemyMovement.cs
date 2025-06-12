using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(NavMeshAgent), typeof(LungeAttack))]
public class EnemyMovement : MonoBehaviour
{
    public Transform player;

    public float stopDistance = 1.3f;
    public float rotationSpeed = 10f;
    public Boolean patrol = false;
    public Boolean goingHome = false;
    [SerializeField] LungeAttack attackScript;
    [ReadOnlyAtribute][SerializeField] private bool canMove = true;


    public graphPath path;
    public List<ZoneType> interests = new List<ZoneType>();
    public List<ZoneType> fears = new List<ZoneType>();
    public Boolean faceYourFears = true;
    public int homeID;
    public List<ZoneType> visitedInterests = new List<ZoneType>();

    public List<int> vertexPath = new List<int>();
    int indexOnPath = -1;

    [ReadOnlyAtribute][SerializeField] Boolean followingPath = false;


    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        if (canMove)
        {
            if (patrol)
            {
                behaviorController();
            }
            else
            {
                moveToPlayer();
            }
        }

        HandleRotation();
    }

    void OnEnable()
    {
        if (attackScript != null)
        {
            attackScript.OnAttackSequenceStart.AddListener(HandleAttackStarted);
            attackScript.OnAttackSequenceEnd.AddListener(HandleAttackFinished);
        }
    }

    void OnDisable()
    {
        if (attackScript != null)
        {
            attackScript.OnAttackSequenceStart.RemoveListener(HandleAttackStarted);
            attackScript.OnAttackSequenceEnd.RemoveListener(HandleAttackFinished);
        }
    }

    public void HandleAttackStarted()
    {

        Debug.Log("asasdasd");
        canMove = false;
        agent.ResetPath();
    }

    public void HandleAttackFinished()
    {
        canMove = true;
        agent.isStopped = false;
    }






    public void moveToVertex()
    {
        if (indexOnPath == -1 || (path.graphMap.GetVertex(vertexPath[indexOnPath]).position - transform.position).magnitude < 1)
        {
            indexOnPath++;
            if (indexOnPath >= vertexPath.Count)
            {
                followingPath = false;
                visitedInterests.Add(path.graphMap.GetVertex(vertexPath[indexOnPath - 1]).zoneType);
                return;
            }
            agent.SetDestination(path.graphMap.GetVertex(vertexPath[indexOnPath]).position);
        }
    }
    public void behaviorController()
    {
        if (followingPath)
        {
            moveToVertex();
        }
        else
        {
            indexOnPath = -1;
            int ret = findNextInterest();
            if (ret == -1)
            {
                agent.SetDestination(path.graphMap.GetVertex(homeID).position);
                goingHome = true;
            }
            else
            {
                followingPath = true;
            }
        }
        if (goingHome && (path.graphMap.GetVertex(homeID).position - transform.position).magnitude < 1)
        {
            goingHome = false;
            followingPath = false;
            visitedInterests.Clear();
        }
    }

    public int findNextInterest()
    {

        if (interests.Count == visitedInterests.Count) return -1;

        HashSet<ZoneType> hi = new HashSet<ZoneType>();
        HashSet<ZoneType> hf = new HashSet<ZoneType>();

        foreach (ZoneType i in interests)
        {
            if (!visitedInterests.Contains(i))
            {
                hi.Add(i);

            }
        }
        foreach (ZoneType i in fears)
        {
            hf.Add(i);
        }

        vertexPath = path.FindInterestVertex(transform.position, hi, hf, faceYourFears);

        if (vertexPath.Count == 0)
        {
            return -1;
        }
        else
        {
            return 0;
        }


    }




    private void moveToPlayer()
    {
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance > stopDistance)
            {
                agent.isStopped = false;
                if (agent.isOnNavMesh)
                {
                    agent.SetDestination(player.position);
                }
            }
            else
            {
                agent.isStopped = true;
                // agent.ResetPath(); // Opcional, isStopped = true pode ser suficiente
            }


            HandleRotation();
        }
    }

    void HandleRotation()
    {
        // Só rotaciona se o agente estiver se movendo (não parado)
        // e tiver uma velocidade detectável.
        if (!agent.isStopped && agent.velocity.sqrMagnitude > 0.01f)
        {
            // A direção da velocidade do NavMeshAgent
            Vector2 moveDirection = new Vector2(agent.velocity.x, agent.velocity.y);

            if (moveDirection != Vector2.zero)
            {
                // Calcula o ângulo em graus. Atan2 lida com todos os quadrantes corretamente.
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

                // --- AJUSTE O OFFSET DO ÂNGULO AQUI ---
                // Isso depende de qual lado do seu sprite é considerado "frente".
                // Se o seu sprite, sem rotação (0 graus na Unity), aponta para a DIREITA: offsetAngle = 0;
                // Se o seu sprite, sem rotação, aponta para CIMA: offsetAngle = -90f; (ou +270f)
                // Se o seu sprite, sem rotação, aponta para ESQUERDA: offsetAngle = 180f;
                // Se o seu sprite, sem rotação, aponta para BAIXO: offsetAngle = 90f;
                float offsetAngle = -90f; // Exemplo: Se a "frente" do seu sprite é a parte de CIMA dele.

                // Cria a rotação alvo no eixo Z
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + offsetAngle);

                // Rotaciona suavemente em direção à rotação alvo
                // Se quiser rotação instantânea, use: transform.rotation = targetRotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        // Opcional: Se você quiser que o inimigo encare o jogador quando estiver parado
        else if (agent.isStopped && player != null)
        {
            Vector2 directionToPlayer = player.position - transform.position;
            float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            float offsetAngle = -90f; // Ajuste conforme o seu sprite (mesmo offset de cima)
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angleToPlayer + offsetAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}