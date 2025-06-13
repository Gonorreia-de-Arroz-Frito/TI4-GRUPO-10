using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [SerializeField] float maxFood;
    [SerializeField][ReadOnlyAtribute] float fome;
    [SerializeField] float foodDepleteRate;
    [SerializeField] float foodReplenishRate;


    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        fome = maxFood;
    }

    enum behaviourMode
    {
        patrol,
        goToFood,
        goToPlayer,
        goToHome,
        eating
    }

    [SerializeField][ReadOnlyAtribute] behaviourMode currentMode = behaviourMode.patrol;


    // _________________________________________________________________________
    // Add buttons in the Inspector to activate each mode setter
    [ContextMenu("Set Patrol Mode")]
    public void EditorSetModePatrol() => SetModePatrol();

    [ContextMenu("Set Go To Player Mode")]
    public void EditorSetModeGoToPlayer() => SetModeGoToPlayer();

    [ContextMenu("Set Go To Food Mode")]
    public void EditorSetModeGoToFood() => SetModeGoToFood();

    [ContextMenu("Set Go To Home Mode")]
    public void EditorSetModeGoToHome() => setModeGoToHome();

    [ContextMenu("Set Eating Mode")]
    public void EditorSetModeEating() => setModeEating();

#if UNITY_EDITOR
    [CustomEditor(typeof(EnemyMovement))]
    public class EnemyMovementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EnemyMovement script = (EnemyMovement)target;

            GUILayout.Space(10);
            GUILayout.Label("Debug Mode Setters", EditorStyles.boldLabel);

            if (GUILayout.Button("Set Patrol Mode"))
            {
                script.EditorSetModePatrol();
            }
            if (GUILayout.Button("Set Go To Player Mode"))
            {
                script.EditorSetModeGoToPlayer();
            }
            if (GUILayout.Button("Set Go To Food Mode"))
            {
                script.EditorSetModeGoToFood();
            }
            if (GUILayout.Button("Set Go To Home Mode"))
            {
                script.EditorSetModeGoToHome();
            }
            if (GUILayout.Button("Set Eating Mode"))
            {
                script.EditorSetModeEating();
            }
        }
    }
#endif


    void Update()
    {
        HandleRotation();
        if (canMove)
        {
            switch (currentMode)
            {
                case behaviourMode.patrol:
                    PatrolBehavior();
                    break;
                case behaviourMode.goToFood:
                    behaviorController(new List<ZoneType> { ZoneType.Food });
                    break;
                case behaviourMode.goToPlayer:
                    moveToPlayer();
                    break;
                case behaviourMode.goToHome:
                    homeBehaviour();
                    break;
                case behaviourMode.eating:
                    eat();
                    break;
            }
        }
        if (currentMode != behaviourMode.eating)
        {
            if (fome < maxFood)
            {
                fome += foodDepleteRate * Time.deltaTime;
            }
        }
    }


    // ___________________________________________
    // set mode for patrol player and food
    public void SetModePatrol()
    {
        resetBehaviour();
        currentMode = behaviourMode.patrol;
    }

    public void SetModeGoToPlayer()
    {
        resetBehaviour();
        currentMode = behaviourMode.goToPlayer;
    }

    public void SetModeGoToFood()
    {
        resetBehaviour();
        currentMode = behaviourMode.goToFood;
    }

    public void setModeGoToHome()
    {
        resetBehaviour();
        currentMode = behaviourMode.goToHome;
    }

    public bool eating;
    public void setModeEating()
    {
        resetBehaviour();
        eating = true;
        currentMode = behaviourMode.eating;
    }
    // ____________________________________________

    void resetBehaviour()
    {
        eating = false;
        visitedInterests.Clear();
        followingPath = false;
        agent.isStopped = false;
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

    // ___________________________________________________________________________________________________________

    public float getFome()
    {
        return fome;
    }
    public float getMaxFood()
    {
        return maxFood;
    }

    // ___________________________________________________________________________________________________________

    public void moveToVertex()
    {
        if (
            indexOnPath == -1 ||                                                                           // If indexOnPath is null
            (path.graphMap.GetVertex(vertexPath[indexOnPath]).position - transform.position).magnitude < 1 // if distance between position and target is 1
         )
        {
            // add 1 to index
            indexOnPath++;

            // if finished going trought
            if (indexOnPath >= vertexPath.Count)
            {
                followingPath = false;
                // added this zoneType to list of visited types
                visitedInterests.Add(path.graphMap.GetVertex(vertexPath[indexOnPath - 1]).zoneType);
                return;
            }
            agent.SetDestination(path.graphMap.GetVertex(vertexPath[indexOnPath]).position);
        }
    }
    public void behaviorController(List<ZoneType> it)
    {
        if (followingPath)
        {
            moveToVertex();
        }
        else // if not following
        {
            indexOnPath = -1;
            int ret = findNextInterest(it); // try to find next interests
            if (ret == -1) // if coudnt
            {
                agent.isStopped = true;
            }
            else
            {
                followingPath = true;
            }
        }
    }

    public void PatrolBehavior()
    {
        if (followingPath)
        {
            moveToVertex();
        }
        else // if not following
        {
            indexOnPath = -1;
            int ret = findRandomInterest(); // try to find next interests
            if (ret == -1) // if coudnt
            {
                resetBehaviour();
            }
            else
            {
                followingPath = true;
            }
        }
    }

    public void homeBehaviour()
    {
        if (followingPath)
        {
            moveToVertex();
        }
        else // if not following
        {
            indexOnPath = -1;
            int ret = findSpecificInterest(homeID); // try to find next interests
            if (ret == -1) // if coudnt
            {
                agent.isStopped = true;
            }
            else
            {
                followingPath = true;
            }
        }
    }

    public void eat()
    {
        agent.isStopped = true;
        if (fome > 0)
        {
            fome -= foodReplenishRate * Time.deltaTime;
        }
        else
        {
            fome = 0;
        }
    }

    public int findSpecificInterest(int id)
    {
        HashSet<ZoneType> hf = new HashSet<ZoneType>();

        foreach (ZoneType i in fears)
        {
            hf.Add(i);
        }

        vertexPath = path.FindSpecificVertex(
            transform.position,
            id,
            hf,
            faceYourFears
            );

        if (vertexPath.Count == 0)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public int findRandomInterest()
    {
        HashSet<ZoneType> hf = new HashSet<ZoneType>();

        foreach (ZoneType i in fears)
        {
            hf.Add(i);
        }

        vertexPath = path.FindSpecificVertex(
            transform.position,
            path.graphMap.GetRandomVertex().id,
            hf,
            faceYourFears
            );

        if (vertexPath.Count == 0)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public int findNextInterest(List<ZoneType> it)
    {

        if (it.Count == visitedInterests.Count) return -1;

        HashSet<ZoneType> hi = new HashSet<ZoneType>();
        HashSet<ZoneType> hf = new HashSet<ZoneType>();

        foreach (ZoneType i in it)
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