using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Interact))]
public class SingleLayerExampleEditor : Editor
{
    SerializedProperty layerProp;

    void OnEnable()
    {
        layerProp = serializedObject.FindProperty("temporaryLayer");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        // Draw default fields, except "layer"
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (prop.name == "temporaryLayer")
            {
                // Replace with single-layer picker
                layerProp.intValue = EditorGUILayout.LayerField("Temporary Layer", layerProp.intValue);
            }
            else
            {
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }
}