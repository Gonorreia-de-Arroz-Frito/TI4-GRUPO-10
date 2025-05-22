using UnityEngine;
using System.Collections.Generic;
using System;

// ---------- GraphMap ----------
[Serializable]
public class GraphMap
{
    [Serializable]
    public struct Entry
    {
        public int Key;
        public Vertex Value;

        public Entry(int key, Vertex value)
        {
            Key = key;
            Value = value;
        }
    }

    [SerializeField]
    private List<Entry> entries = new List<Entry>();

    private Dictionary<int, Vertex> internalDictionary;

    public void BuildDictionary()
    {
        internalDictionary = new Dictionary<int, Vertex>();
        foreach (var entry in entries)
        {
            internalDictionary[entry.Key] = entry.Value;
        }
    }

    public void Put(int key, Vertex value)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (EqualityComparer<int>.Default.Equals(entries[i].Key, key))
            {
                entries[i] = new Entry(key, value);
                return;
            }
        }
        entries.Add(new Entry(key, value));
    }

    public bool TryGetValue(int key, out Vertex value)
    {
        foreach (var entry in entries)
        {
            if (EqualityComparer<int>.Default.Equals(entry.Key, key))
            {
                value = entry.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    public Dictionary<int, Vertex> ToDictionary()
    {
        BuildDictionary();
        return internalDictionary;
    }
}

// ---------- Enums ----------
public enum ZoneType
{
    Neutral,
    SafeZone,
    DangerZone,
    PatrolZone,
    ObjectiveZone
}

public enum EnemyBehaviorType
{
    Neutral,
    Cautious,    // Evita DangerZone, prefere SafeZone
    Aggressive   // Ignora DangerZone, prioriza ObjectiveZone
}

// ---------- Vertex ----------
[System.Serializable]
public class Vertex
{
    [SerializeField]
    public int id;
    [SerializeField]
    public string name;

    [SerializeField]
    public Vector3 position;
    [SerializeField]
    public List<int> neighbors;

    public ZoneType zoneType = ZoneType.Neutral;

    public Vertex(int id, Vector3 position)
    {
        this.id = id;
        this.position = position;
        neighbors = new List<int>();
    }
}

// ---------- graphPath ----------
public class graphPath : MonoBehaviour
{
    [SerializeField] public bool AlingToGrid = true;
    [SerializeField] public float vertexSize = 0.1f;

    [SerializeField] public List<Vertex> AdjacencyList = new List<Vertex>();
    public int vertexCount = 0;
    public int vertedLastID = 0;

    void Start() { }

    void Update() { }

    // ---------- Algoritmo de Custo ----------
    public float GetCost(Vertex from, Vertex to, EnemyBehaviorType behavior)
    {
        float baseCost = Vector3.Distance(from.position, to.position);

        if (to.zoneType == ZoneType.DangerZone)
        {
            if (behavior == EnemyBehaviorType.Cautious)
                baseCost *= 5f;
        }
        else if (to.zoneType == ZoneType.SafeZone)
        {
            if (behavior == EnemyBehaviorType.Cautious)
                baseCost *= 0.5f;
        }

        return baseCost;
    }

    // ---------- Algoritmo A* ----------
    public List<Vertex> FindPath(int startId, int goalId, EnemyBehaviorType behavior)
    {
        var frontier = new PriorityQueue<Vertex>();
        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        Dictionary<int, float> costSoFar = new Dictionary<int, float>();

        Vertex start = AdjacencyList.Find(v => v.id == startId);
        Vertex goal = AdjacencyList.Find(v => v.id == goalId);

        if (start == null || goal == null)
        {
            Debug.LogWarning("Start or Goal vertex not found.");
            return new List<Vertex>();
        }

        frontier.Enqueue(start, 0);
        cameFrom[start.id] = start.id;
        costSoFar[start.id] = 0;

        while (frontier.Count > 0)
        {
            Vertex current = frontier.Dequeue();

            if (current.id == goal.id)
                break;

            foreach (int neighborId in current.neighbors)
            {
                Vertex neighbor = AdjacencyList.Find(v => v.id == neighborId);
                float newCost = costSoFar[current.id] + GetCost(current, neighbor, behavior);

                if (!costSoFar.ContainsKey(neighbor.id) || newCost < costSoFar[neighbor.id])
                {
                    costSoFar[neighbor.id] = newCost;
                    float priority = newCost;
                    frontier.Enqueue(neighbor, priority);
                    cameFrom[neighbor.id] = current.id;
                }
            }
        }

        // Reconstruir caminho
        List<Vertex> path = new List<Vertex>();
        int currentId = goal.id;

        if (!cameFrom.ContainsKey(goal.id))
        {
            Debug.LogWarning("Path not found!");
            return path;
        }

        while (currentId != start.id)
        {
            Vertex currentVertex = AdjacencyList.Find(v => v.id == currentId);
            path.Add(currentVertex);
            currentId = cameFrom[currentId];
        }

        path.Add(start);
        path.Reverse();

        return path;
    }

    // ---------- Gizmos ----------
    void OnDrawGizmos()
    {
        /*
        if (AdjacencyList == null || AdjacencyList.Count == 0)
            return;

        for (int i = 0; i < AdjacencyList.Count; i++)
        {
            if (AlingToGrid)
            {
                AdjacencyList[i].position.x = Mathf.Round(AdjacencyList[i].position.x - 0.5f) + 0.5f;
                AdjacencyList[i].position.y = Mathf.Round(AdjacencyList[i].position.y - 0.5f) + 0.5f;
                AdjacencyList[i].position.z = Mathf.Round(AdjacencyList[i].position.z - 0.5f) + 0.5f;
                AlingToGrid = false;
            }

            for (int j = 0; j < AdjacencyList[i].neighbors.Count; j++)
            {
                int neighborId = AdjacencyList[i].neighbors[j];

                if (neighborId < AdjacencyList.Count && neighborId > i)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(AdjacencyList[i].position, AdjacencyList[neighborId].position);
                }
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(AdjacencyList[i].position, vertexSize);
        }
        */
    }
}
