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
            List<Cell> targetCells = new List<Cell>();
            foreach (Cell targetCell in targets) {
                targetCells.Add(targetCell);
                targetCell.CellInfoChanged();
            }
            Undo.RecordObjects(targetCells.ToArray(), "Cell state changes");
        }
    }
}