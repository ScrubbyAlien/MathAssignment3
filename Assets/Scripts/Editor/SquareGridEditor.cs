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

        string generateButtonString;
        if (grid.sizeChanged) generateButtonString = "Regenerate Grid";
        else generateButtonString = "Generate Grid";

        if (GUILayout.Button(generateButtonString)) {
            grid.GenerateGrid();
        }
        if (GUILayout.Button("Clear Grid")) {
            grid.ClearCells(true);
        }
    }
}