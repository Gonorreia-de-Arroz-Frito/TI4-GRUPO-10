using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyPatrol : MonoBehaviour
{
    public graphPath graph;                // Referência ao seu grafo
    public int startNodeId = 0;            // ID do nó inicial
    public float moveSpeed = 2f;
    public float waitTime = 1f;            // Tempo parado entre os movimentos
    public bool randomPatrol = false;      // Alternar entre patrulha linear ou aleatória

    private int currentNodeId;
    private bool isMoving = false;

    void Start()
    {
        currentNodeId = startNodeId;
        transform.position = graph.AdjacencyList[currentNodeId].position;
        StartCoroutine(PatrolRoutine());
    }

    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (!isMoving)
            {
                int nextNodeId = GetNextNode();

                yield return StartCoroutine(MoveTo(graph.AdjacencyList[nextNodeId].position));

                currentNodeId = nextNodeId;
                yield return new WaitForSeconds(waitTime);
            }

            yield return null;
        }
    }

    int GetNextNode()
    {
        List<int> neighbors = graph.AdjacencyList[currentNodeId].neighbors;

        if (neighbors == null || neighbors.Count == 0)
        {
            Debug.LogWarning("Nó sem vizinhos!");
            return currentNodeId;
        }

        if (randomPatrol)
        {
            return neighbors[Random.Range(0, neighbors.Count)];
        }
        else
        {
            // Caminho sequencial (exemplo: sempre pega o próximo vizinho)
            return neighbors[0]; // ou implemente uma lógica de rotatividade
        }
    }

    IEnumerator MoveTo(Vector3 target)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target; // garante posição exata
        isMoving = false;
    }
}
