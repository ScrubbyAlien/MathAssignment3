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
        else generateButtonString = "Clear grid";

        if (GUILayout.Button(generateButtonString)) {
            grid.GenerateGrid();
        }
    }
}