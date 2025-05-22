using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    public graphPath graph;
    public int startId;
    public int goalId;
    public EnemyBehaviorType behavior = EnemyBehaviorType.Cautious;

    private List<Vertex> path;
    private int currentIndex = 0;

    public float speed = 2f;

    void Start()
    {
        // Calcula o caminho
        path = graph.FindPath(startId, goalId, behavior);
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("No path found!");
        }
    }

    void Update()
    {
        if (path == null || currentIndex >= path.Count)
            return;

        Vector3 targetPos = path[currentIndex].position;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            currentIndex++;
        }
    }
}
