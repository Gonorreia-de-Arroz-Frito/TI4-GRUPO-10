using UnityEngine;
using System.Collections.Generic;
using System;


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

    public Vertex(int id, Vector3 position)
    {
        this.id = id;
        this.position = position;
        neighbors = new List<int>();
    }
}

public class graphPath : MonoBehaviour
{

    [SerializeField] public bool AlingToGrid = true;
    [SerializeField] public float vertexSize = 0.1f;

    [SerializeField] public List<Vertex> AdjacencyList = new List<Vertex>();
    public int vertexCount = 0;
    public int vertedLastID = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

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
            Gizmos.DrawSphere(AdjacencyList[i].position, vertexRadius);

        }
        */
    }
}