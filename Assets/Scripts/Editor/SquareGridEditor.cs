using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SquareGrid))]
public class SquareGridEditor : Editor
{
    private SquareGrid grid;

    private void OnEnable() {
        grid = target as SquareGrid;
    }

    /// <inheritdoc />
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Grid")) {
            grid.GenerateGrid();
        }
        if (GUILayout.Button("Clear Grid")) {
            grid.ClearCells(true);
        }
    }
}