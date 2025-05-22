using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float stopDistance = 1.3f;
    public float rotationSpeed = 10f; 

    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; 
        agent.updateUpAxis = false;   
    }

    void Update()
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
                
            }

            
            HandleRotation();
        }
    }

    void HandleRotation()
    {


        if (agent.isStopped == false && agent.velocity.sqrMagnitude > 0.01f)
        {
            
            Vector2 moveDirection = new Vector2(agent.velocity.x, agent.velocity.y);

            if (moveDirection != Vector2.zero)
            {
                
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

                float offsetAngle = -90f;

                // Z axis rotation
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + offsetAngle);

                // Rotate to the target rotation
                // transform.rotation = targetRotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
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