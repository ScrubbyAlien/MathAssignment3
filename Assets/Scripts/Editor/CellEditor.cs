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
    private SerializedProperty obstacleGradient;

    /// <inheritdoc />
    public override void OnInspectorGUI() {
        info = serializedObject.FindProperty("info");
        stateObjects = serializedObject.FindProperty("stateObjects");
        coordsSet = serializedObject.FindProperty("coordsSet");
        obstacleGradient = serializedObject.FindProperty("obstacleGradient");

        bool instantiated = coordsSet.boolValue;
        EditorGUILayout.PropertyField(info);

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        if (!instantiated) {
            EditorGUILayout.PropertyField(obstacleGradient);
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