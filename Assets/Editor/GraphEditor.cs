using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Editor personalizado do Unity para o componente <c>graphPath</c>.
/// Permite criar e editar vértices, tipos, zonas e conexões diretamente pela Scene e Inspector.
/// </summary>
[CustomEditor(typeof(graphPath))]
public class GraphEditor : Editor
{
    private graphPath graph;
    private const float clickRadius = 0.5f;

    int dropdownFrom = 0; // Dropdown de origem manual
    int dropdownTo = 0;   // Dropdown de destino manual

    /// <summary>
    /// Obtém referência para o grafo quando o editor for ativado.
    /// </summary>
    private void OnEnable()
    {
        graph = (graphPath)target;
    }

    /// <summary>
    /// Manipula eventos e handles da Scene View (posicionamento e conexões).
    /// </summary>
    private void OnSceneGUI()
    {
        Event e = Event.current;
        Handles.color = Color.cyan;

        var vertices = graph.AdjacencyList;

        // Desenha linhas entre vizinhos
        foreach (var v in vertices)
        {
            foreach (int neighborId in v.neighbors)
            {
                var neighbor = vertices.FirstOrDefault(n => n.id == neighborId);
                if (neighbor != null && neighbor.id > v.id)
                {
                    Handles.DrawLine(v.position, neighbor.position);
                }
            }
        }

        // Permite movimentar vértices e selecionar
        for (int i = 0; i < vertices.Count; i++)
        {
            Vertex v = vertices[i];

            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(v.position, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(graph, "Move Vertex");
                v.position = newPos;
                graph.graphMap.Put(v.id, v);
                EditorUtility.SetDirty(graph);
            }

            Handles.color = graph.selectedVertices.Contains(i) ? Color.cyan : Color.red;
            if (vertices[i].useColor)
            {
                Handles.color = vertices[i].color;
            }

            if (Handles.Button(v.position, Quaternion.identity, graph.vertexSize, 0.2f, Handles.SphereHandleCap))
            {
                if (Event.current.control)
                {
                    if (!graph.selectedVertices.Contains(i))
                        graph.selectedVertices.Add(i);
                    else
                        graph.selectedVertices.Remove(i);
                }
                else
                {
                    graph.selectedVertices.Clear();
                    graph.selectedVertices.Add(i);
                }

                GUI.changed = true;
            }
        }
    }

    /// <summary>
    /// Interface personalizada do Inspector.
    /// Permite adicionar vértices, modificar tipo/zona e conectar nós.
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Graph Editor Tools", EditorStyles.boldLabel);

        // Botão para adicionar novo vértice
        if (GUILayout.Button("Add Vertex"))
        {
            Undo.RecordObject(graph, "Add Vertex");
            var newVertex = new Vertex(graph.vertedLastID++, graph.transform.position);
            graph.graphMap.Put(newVertex.id, newVertex);
            graph.vertexCount++;
            EditorUtility.SetDirty(graph);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Zone Type Modifier", EditorStyles.boldLabel);

        // Permite mudar zona e tipo de um vértice
        if (graph.selectedVertices.Count == 1)
        {
            var v = graph.AdjacencyList[graph.selectedVertices[0]];

            ZoneType newType = (ZoneType)EditorGUILayout.EnumPopup("Zone Type", v.zoneType);
            if (newType != v.zoneType)
            {
                Undo.RecordObject(graph, "Change Zone Type");
                v.zoneType = newType;
                graph.graphMap.Put(v.id, v);
                EditorUtility.SetDirty(graph);
            }

            /*
            int newTipo = EditorGUILayout.IntField("Tipo", v.tipo);
            if (newTipo != v.tipo)
            {
                Undo.RecordObject(graph, "Change Tipo");
                v.tipo = newTipo;
                graph.graphMap.Put(v.id, v);
                EditorUtility.SetDirty(graph);
            }
            */
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Edge Tools", EditorStyles.boldLabel);

        // Botão para criar aresta entre dois vértices selecionados
        if (graph.selectedVertices.Count == 2)
        {
            if (GUILayout.Button("Add Edge Between Selected"))
            {
                var a = graph.AdjacencyList[graph.selectedVertices[0]];
                var b = graph.AdjacencyList[graph.selectedVertices[1]];

                if (!a.neighbors.Contains(b.id)) a.neighbors.Add(b.id);
                if (!b.neighbors.Contains(a.id)) b.neighbors.Add(a.id);

                graph.graphMap.Put(a.id, a);
                graph.graphMap.Put(b.id, b);
                EditorUtility.SetDirty(graph);
                graph.selectedVertices.Clear();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Select 2 vertices in the Scene View with Ctrl + Click to add an edge.", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Add Edge Manually", EditorStyles.boldLabel);

        // Interface para conexão manual
        string[] vertexNames = graph.AdjacencyList.Select(v => v.name).ToArray();
        if (vertexNames.Length >= 2)
        {
            dropdownFrom = EditorGUILayout.Popup("From", dropdownFrom, vertexNames);
            dropdownTo = EditorGUILayout.Popup("To", dropdownTo, vertexNames);

            if (dropdownFrom != dropdownTo)
            {
                if (GUILayout.Button($"Add Edge: {vertexNames[dropdownFrom]} → {vertexNames[dropdownTo]}"))
                {
                    var a = graph.AdjacencyList[dropdownFrom];
                    var b = graph.AdjacencyList[dropdownTo];

                    if (!a.neighbors.Contains(b.id)) a.neighbors.Add(b.id);
                    if (!b.neighbors.Contains(a.id)) b.neighbors.Add(a.id);

                    graph.graphMap.Put(a.id, a);
                    graph.graphMap.Put(b.id, b);
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

        EditorGUILayout.Space();
        if (EditorGUILayout.LinkButton("TestPath from 1 to Type 2 not going though Type 3"))
        {
            HashSet<ZoneType> hI = new HashSet<ZoneType>();
            hI.Add(ZoneType.Normal);
            HashSet<ZoneType> hB = new HashSet<ZoneType>();
            hB.Add(ZoneType.Food);



            List<int> list = graph.FindInterestVertex(new Vector2(0, 0), hI, hB, false);

            string result = string.Join(" --> ", list.Select(id => id.ToString()));
            Debug.Log(result);
        }
    }
}
