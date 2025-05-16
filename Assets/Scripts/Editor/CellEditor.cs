using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cell)), CanEditMultipleObjects]
public class CellEditor : Editor
{
    private SerializedProperty info;
    private SerializedProperty stateObjects;
    private SerializedProperty highlightMarker;
    private SerializedProperty coordsSet;

    /// <inheritdoc />
    public override void OnInspectorGUI() {
        info = serializedObject.FindProperty("info");
        stateObjects = serializedObject.FindProperty("stateObjects");
        highlightMarker = serializedObject.FindProperty("highlightMarker");
        coordsSet = serializedObject.FindProperty("coordsSet");
        bool instantiated = coordsSet.boolValue;
        EditorGUILayout.PropertyField(info);

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        if (!instantiated) {
            EditorGUILayout.PropertyField(highlightMarker);
            EditorGUILayout.PropertyField(stateObjects);
        }

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