using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Tipos de zona de um vértice.
/// </summary>
public enum ZoneType
{
    Normal,     // Caminho livre
    Blocked     // Caminho bloqueado (obstáculo)
}

/// <summary>
/// Representa um vértice no grafo.
/// </summary>
[System.Serializable]
public class Vertex
{
    public int id;                       // Identificador único
    public string name;                 // Nome do vértice (ex: V1)
    public Vector3 position;           // Posição no espaço
    public List<int> neighbors = new(); // IDs dos vizinhos conectados
    public ZoneType zoneType = ZoneType.Normal; // Tipo de zona
    [SerializeField]
    public int tipo = 0;               // Tipo customizável
    public bool useColor = false;
    public Color color = Color.white;

    public Vertex(int id, Vector3 position)
    {
        this.id = id;
        this.position = position;
        this.name = $"V{id}";
    }
}

/// <summary>
/// Mapeia os vértices com suporte à serialização para o Unity.
/// </summary>
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
    private List<Entry> entries = new();

    private Dictionary<int, Vertex> internalDictionary;

    /// <summary>
    /// Constrói o dicionário interno a partir da lista serializada.
    /// </summary>
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
            if (entries[i].Key == key)
            {
                entries[i] = new Entry(key, value);
                BuildDictionary();
                return;
            }
        }
        entries.Add(new Entry(key, value));
        BuildDictionary();
    }

    public Vertex GetVertex(int key)
    {
        if (internalDictionary == null)
            BuildDictionary();

        if (internalDictionary.TryGetValue(key, out Vertex value))
            return value;
        else
            return null;
    }
    public bool TryGetValue(int key, out Vertex value)
    {
        if (internalDictionary == null)
            BuildDictionary();

        return internalDictionary.TryGetValue(key, out value);
    }

    public Dictionary<int, Vertex> ToDictionary()
    {
        BuildDictionary();
        return internalDictionary;
    }

    public List<Vertex> GetAllVertices()
    {
        BuildDictionary();
        return new List<Vertex>(internalDictionary.Values);
    }
}

/// <summary>
/// Fila de prioridade para o algoritmo A*.
/// </summary>
public class PriorityQueue<T>
{
    private List<KeyValuePair<T, float>> elements = new();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add(new KeyValuePair<T, float>(item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].Value < elements[bestIndex].Value)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Key;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}

/// <summary>
/// Script principal do grafo com suporte a tipos e algoritmos de caminho.
/// </summary>
public class graphPath : MonoBehaviour
{
    [Header("Graph Settings")]
    public GraphMap graphMap = new();

    [Header("Gizmo Settings")]
    public float vertexSize = 0.2f;
    public Color normalColor = Color.green;
    public Color blockedColor = Color.red;
    public Color lineColor = Color.cyan;

    [HideInInspector] public int vertexCount = 0;
    [HideInInspector] public int vertedLastID = 0;

    public List<int> selectedVertices = new();

    public List<Vertex> AdjacencyList => graphMap.GetAllVertices();

    public bool vaiLaFilhao = false;

    void Awake()
    {
        graphMap.BuildDictionary();
    }

    /// <summary>
    /// Encontra o vértice mais próximo (não bloqueado).
    /// </summary>
    public Vertex FindClosestVertex(Vector3 position)
    {
        Vertex closest = null;
        float minDist = Mathf.Infinity;

        foreach (var vertex in graphMap.GetAllVertices())
        {
            if (vertex.zoneType == ZoneType.Blocked)
                continue;

            float dist = Vector3.Distance(position, vertex.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = vertex;
            }
        }

        return closest;
    }

    /// <summary>
    /// Exibe visualmente os vértices e conexões no editor.
    /// </summary>
    void OnDrawGizmos()
    {
        if (graphMap == null)
            return;

        graphMap.BuildDictionary();
        var vertices = graphMap.GetAllVertices();

        foreach (var vertex in vertices)
        {
            foreach (int neighborId in vertex.neighbors)
            {
                if (graphMap.TryGetValue(neighborId, out Vertex neighbor))
                {
                    Gizmos.color = lineColor;
                    Gizmos.DrawLine(vertex.position, neighbor.position);
                }
            }

            Gizmos.color = vertex.zoneType == ZoneType.Blocked ? blockedColor : normalColor;
            Gizmos.DrawSphere(vertex.position, vertexSize);
        }
    }

    /// <summary>
    /// Altera o tipo de um vértice específico.
    /// </summary>
    public void SetVertexTipo(int vertexId, int novoTipo)
    {
        if (graphMap.TryGetValue(vertexId, out Vertex v))
        {
            v.tipo = novoTipo;
            graphMap.Put(vertexId, v);
        }
    }

    /// <summary>
    /// Encontra o vértice mais próximo de um tipo específico.
    /// </summary>
    public Vertex FindClosestVertexOfType(Vector3 position, int tipoDesejado)
    {
        Vertex closest = null;
        float minDist = Mathf.Infinity;

        foreach (var vertex in graphMap.GetAllVertices())
        {
            if (vertex.tipo != tipoDesejado || vertex.zoneType == ZoneType.Blocked)
                continue;

            float dist = Vector3.Distance(position, vertex.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = vertex;
            }
        }

        return closest;
    }

    /// <summary>
    /// Encontra o caminho até o vértice mais próximo que pertença a um dos tipos de interesse,
    /// evitando tipos de vértices proibidos. Se nenhum caminho for encontrado e a flag `tryIgnore`
    /// for verdadeira, tenta novamente ignorando os tipos proibidos.
    ///
    /// Essa função combina a lógica de busca com A* e filtragem por tipo de vértice.
    ///
    /// Exemplo:
    /// - Interesse: encontrar o vértice mais próximo do tipo "Item" (ex: tipo 2 ou 3)
    /// - Evitar: caminhos que passem por vértices do tipo "Perigo" (ex: tipo 5)
    /// - Se não encontrar, pode tentar novamente ignorando os perigos (caso `tryIgnore` = true)
    /// </summary>
    /// <param name="startPosition">Posição de onde a busca começa</param>
    /// <param name="interestTypes">Conjunto de tipos de vértices que são objetivo (ex: {2, 3})</param>
    /// <param name="prohibitedTypes">Conjunto de tipos de vértices a evitar (ex: {5})</param>
    /// <param name="tryIgnore">Se verdadeiro, tenta uma segunda busca ignorando os proibidos</param>
    /// <returns>Lista de IDs dos vértices que formam o caminho até o objetivo mais próximo, ou lista vazia</returns>
    public List<int> FindInterestVertex(Vector3 startPosition, HashSet<int> interestTypes, HashSet<int> prohibitedTypes, bool tryIgnore)
    {
        // Função interna para fazer a busca com os tipos proibidos informados
        List<int> TryFind(HashSet<int> tiposProibidos)
        {
            // Encontra o vértice mais próximo que não esteja bloqueado
            Vertex start = FindClosestVertex(startPosition);
            if (start == null)
                return new();

            var frontier = new PriorityQueue<Vertex>();         // Fila de prioridade (para o A*)
            Dictionary<int, int> cameFrom = new();              // Dicionário de predecessores
            Dictionary<int, float> costSoFar = new();           // Custo acumulado do caminho

            frontier.Enqueue(start, 0);
            cameFrom[start.id] = start.id;
            costSoFar[start.id] = 0;

            while (frontier.Count > 0)
            {
                Vertex current = frontier.Dequeue();

                // Se o vértice atual é de um tipo de interesse, retornamos o caminho até ele
                if (interestTypes.Contains(current.tipo))
                    return ReconstructPathIDs(cameFrom, start.id, current.id);

                // Explora os vizinhos do vértice atual
                foreach (int neighborId in current.neighbors)
                {
                    if (!graphMap.TryGetValue(neighborId, out Vertex neighbor))
                        continue;

                    // Ignora vizinhos que estão bloqueados ou são de tipos proibidos
                    if (neighbor.zoneType == ZoneType.Blocked || tiposProibidos.Contains(neighbor.tipo))
                        continue;

                    // Calcula o custo até o vizinho
                    float newCost = costSoFar[current.id] + Vector3.Distance(current.position, neighbor.position);

                    // Se for um caminho mais curto ou ainda não visitado, atualiza
                    if (!costSoFar.ContainsKey(neighbor.id) || newCost < costSoFar[neighbor.id])
                    {
                        costSoFar[neighbor.id] = newCost;
                        float priority = newCost + Heuristic(neighbor.position, current.position); // f(n) = g(n) + h(n)
                        frontier.Enqueue(neighbor, priority);
                        cameFrom[neighbor.id] = current.id;
                    }
                }
            }

            // Nenhum caminho até vértice de interesse foi encontrado
            return new();
        }

        // Primeira tentativa com os tipos proibidos
        List<int> result = TryFind(prohibitedTypes);

        // Se não encontrou e deve tentar ignorar os proibidos, faz nova tentativa sem restrição
        if (result.Count == 0 && tryIgnore)
            result = TryFind(new()); // Passa um conjunto vazio (sem proibições)

        return result;
    }

    /// <summary>
    /// Função heurística para A* (distância Euclidiana).
    /// </summary>
    private float Heuristic(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b);
    }

    /// <summary>
    /// Reconstrói o caminho como uma lista de IDs de vértices a partir do dicionário de predecessores.
    /// </summary>
    /// <param name="cameFrom">Dicionário que mapeia cada vértice ao seu predecessor</param>
    /// <param name="startId">ID do vértice inicial</param>
    /// <param name="goalId">ID do vértice objetivo</param>
    /// <returns>Lista com os IDs dos vértices que formam o caminho do início até o objetivo</returns>
    private List<int> ReconstructPathIDs(Dictionary<int, int> cameFrom, int startId, int goalId)
    {
        List<int> path = new();

        if (!cameFrom.ContainsKey(goalId))
            return path; // Caminho impossível

        int currentId = goalId;
        while (currentId != startId)
        {
            path.Add(currentId);
            currentId = cameFrom[currentId];
        }

        path.Add(startId);
        path.Reverse(); // Caminho do início até o fim

        return path;
    }

    /// <summary>
    /// Reconstrói o caminho a partir do dicionário de predecessores.
    /// </summary>
    private List<Vertex> ReconstructPath(Dictionary<int, int> cameFrom, int startId, int goalId)
    {
        List<Vertex> path = new();

        if (!cameFrom.ContainsKey(goalId))
            return path;

        int currentId = goalId;
        while (currentId != startId)
        {
            graphMap.TryGetValue(currentId, out Vertex currentVertex);
            path.Add(currentVertex);
            currentId = cameFrom[currentId];
        }

        graphMap.TryGetValue(startId, out Vertex startVertex);
        path.Add(startVertex);
        path.Reverse();

        return path;
    }
}
