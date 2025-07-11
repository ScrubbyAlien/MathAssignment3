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
        if (grid.sizeChanged) generateButtonString = "Generate Grid";
        else generateButtonString = "Reset grid";

        if (grid.sizeChanged) EditorGUILayout.HelpBox("Please generate grid to see paths", MessageType.Warning);
        if (GUILayout.Button(generateButtonString)) {
            grid.GenerateGrid();
        }
        if (GUILayout.Button("Clear cells")) {
            grid.ClearCells();
        }
    }
}