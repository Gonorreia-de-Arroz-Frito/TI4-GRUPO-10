using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(graphPath))]
public class GraphEditor : Editor
{
    private graphPath graph;
    private const float clickRadius = 0.5f;

    private List<int> selectedVertices = new List<int>();

    int dropdownFrom = 0;
    int dropdownTo = 0;

    private void OnEnable()
    {
        graph = (graphPath)target;
    }

    private void OnSceneGUI()
    {
        Event e = Event.current;
        Handles.color = Color.cyan;

        for (int i = 0; i < graph.AdjacencyList.Count; i++)
        {
            if (graph.AlingToGrid)
            {
                graph.AdjacencyList[i].position.x = Mathf.Round(graph.AdjacencyList[i].position.x - 0.5f) + 0.5f;
                graph.AdjacencyList[i].position.y = Mathf.Round(graph.AdjacencyList[i].position.y - 0.5f) + 0.5f;
                graph.AdjacencyList[i].position.z = Mathf.Round(graph.AdjacencyList[i].position.z - 0.5f) + 0.5f;
            }

            for (int j = 0; j < graph.AdjacencyList[i].neighbors.Count; j++)
            {
                int neighborId = graph.AdjacencyList[i].neighbors[j];

                if (neighborId < graph.AdjacencyList.Count && neighborId > i)
                {
                    Gizmos.color = Color.green;
                    Handles.DrawLine(graph.AdjacencyList[i].position, graph.AdjacencyList[neighborId].position);
                }
            }
        }
        graph.AlingToGrid = false;

        for (int i = 0; i < graph.AdjacencyList.Count; i++)
        {
            Vertex v = graph.AdjacencyList[i];

            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(v.position, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(graph, "Move Vertex");
                v.position = newPos;
                EditorUtility.SetDirty(graph);
            }

            // Draw selection handle
            Handles.color = selectedVertices.Contains(i) ? Color.cyan : Color.red;

            if (Handles.Button(v.position, Quaternion.identity, graph.vertexSize, 0.2f, Handles.SphereHandleCap))
            {
                if (Event.current.control)
                {
                    // Multi-select with Ctrl
                    if (!selectedVertices.Contains(i))
                        selectedVertices.Add(i);
                    else
                        selectedVertices.Remove(i);
                }
                else
                {
                    // Single select
                    selectedVertices.Clear();
                    selectedVertices.Add(i);
                }

                GUI.changed = true;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Graph Editor Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Vertex"))
        {
            Undo.RecordObject(graph, "Add Vertex");
            graph.AdjacencyList.Add(new Vertex(graph.vertedLastID++, new Vector3(0, 0, 0)));
            graph.vertexCount++;
            EditorUtility.SetDirty(graph);
        }


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Edge Tools", EditorStyles.boldLabel);

        if (selectedVertices.Count == 2)
        {
            if (GUILayout.Button($"Add Edge: {graph.AdjacencyList[selectedVertices[0]].name} → {graph.AdjacencyList[selectedVertices[1]].name}"))
            {
                Undo.RecordObject(graph, "Add Edge via Selection");
                graph.AdjacencyList[selectedVertices[0]].neighbors.Add(selectedVertices[1]);
                graph.AdjacencyList[selectedVertices[1]].neighbors.Add(selectedVertices[0]);
                EditorUtility.SetDirty(graph);
                selectedVertices.Clear();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Select 2 vertices in the Scene View with Ctrl + Click to add an edge.", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Add Edge Manually", EditorStyles.boldLabel);

        string[] vertexNames = graph.AdjacencyList.Select(v => v.name).ToArray();
        if (vertexNames.Length >= 2)
        {
            dropdownFrom = EditorGUILayout.Popup("From", dropdownFrom, vertexNames);
            dropdownTo = EditorGUILayout.Popup("To", dropdownTo, vertexNames);

            if (dropdownFrom != dropdownTo)
            {
                if (GUILayout.Button($"Add Edge: {vertexNames[dropdownFrom]} → {vertexNames[dropdownTo]}"))
                {
                    Undo.RecordObject(graph, "Add Edge via Dropdown");

                    graph.AdjacencyList[dropdownFrom].neighbors.Add(dropdownTo);
                    graph.AdjacencyList[dropdownTo].neighbors.Add(dropdownFrom);
                    EditorUtility.SetDirty(graph);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select two different vertices.", MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("At least 2 vertices are required to add an edge.", MessageType.Warning);
        }
        /*
        if (GUILayout.Button("Add Edge (between last two)"))
        {
            if (graph.vertices.Count >= 2)
            {
                Undo.RecordObject(graph, "Add Edge");
                graph.edges.Add(new Edge
                {
                    from = graph.vertices.Count - 2,
                    to = graph.vertices.Count - 1
                });
                EditorUtility.SetDirty(graph);
            }
        }

        if (GUILayout.Button("Clear Graph"))
        {
            if (EditorUtility.DisplayDialog("Clear Graph?", "Are you sure you want to delete all vertices and edges?", "Yes", "Cancel"))
            {
                Undo.RecordObject(graph, "Clear Graph");
                graph.vertices.Clear();
                graph.edges.Clear();
                selectedVertexIndex = -1;
                EditorUtility.SetDirty(graph);
            }
        }
        */
    }
}