using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cell)), CanEditMultipleObjects]
public class CellEditor : Editor
{
    private SerializedProperty info;

    /// <inheritdoc />
    public override void OnInspectorGUI() {
        info = serializedObject.FindProperty("info");
        EditorGUILayout.PropertyField(info);

        if (serializedObject.ApplyModifiedProperties()) {
            foreach (Cell targetCell in targets) {
                Undo.RecordObject(targetCell, "Cell state changes");
                targetCell.CellInfoChanged();
            }
        }
    }
}