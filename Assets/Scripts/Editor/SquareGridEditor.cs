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
        if (LayerMask.NameToLayer("Floor") != 6) {
            EditorGUILayout.HelpBox(
                "To use the palette tool make sure User Layer 6 is set to \"Floor\"",
                MessageType.Warning);
        }

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