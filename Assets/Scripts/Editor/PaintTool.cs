using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ShortcutManagement;
using UnityEngine;

[EditorTool("Paint", typeof(SquareGrid))]
public class PaintTool : EditorTool
{
    public static bool isActive { get; private set; }

    [Shortcut("Activate Paint Tool", KeyCode.U)]
    static void PaintToolShortcut() {
        if (Selection.GetFiltered<SquareGrid>(SelectionMode.TopLevel).Length > 0) {
            ToolManager.SetActiveTool<PaintTool>();
        }
    }

    private Ray ptwr;

    public override void OnToolGUI(EditorWindow window) {
        if (!(window is SceneView)) return;
        isActive = true;

        SceneView.duringSceneGui += UpdateSceneViewMouseRay;

        // https://discussions.unity.com/t/prevent-custom-editortool-from-being-deselected-with-mouse-button-clicks/929028
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        foreach (var obj in targets) {
            if (!(obj is SquareGrid grid)) continue;

            // Vector3 gp = grid.transform.position;
            // Vector3 xAxisStart = new Vector3(gp.x + 0.5f, 0, gp.z - 0.5f);
            // Vector3 xAxisEnd = new Vector3(gp.x + 3.5f, 0, gp.z - 0.5f);
            // Handles.color = Color.red;

            if (!HighlightCurrentCell(ref grid, out Cell currentCell)) continue;

            if (EditorGUI.EndChangeCheck()) { }
        }
    }

    /// <inheritdoc />
    public override void OnWillBeDeactivated() {
        base.OnWillBeDeactivated();
        SceneView.duringSceneGui -= UpdateSceneViewMouseRay;
        isActive = false;
    }

    private bool HighlightCurrentCell(ref SquareGrid grid, out Cell currentCell) {
        currentCell = null;
        if (Physics.Raycast(ptwr, out RaycastHit hitInfo, 1000, LayerMask.GetMask("Floor"))) {
            Vector3 gridSpaceCoords = hitInfo.point - grid.transform.position;
            Vector2 cellCoords = new Vector2(Mathf.Floor(gridSpaceCoords.x), Mathf.Floor(gridSpaceCoords.z));
            currentCell = grid[(int)cellCoords.x, (int)cellCoords.y];

            // midpoint offset and zbuffer artifact handling
            Vector3 markerCenter = new Vector3(cellCoords.x + 0.5f, 0.001f, cellCoords.y + 0.5f);
            Vector3 markerNormal = Vector3.up;
            float markerRadius = 0.3f;

            Handles.color = Color.blue;
            Handles.DrawSolidDisc(markerCenter, markerNormal, markerRadius);

            return true;
        }
        return false;
    }

    // https://discussions.unity.com/t/tracking-the-mouse-position-in-the-editor/7124/2
    private void UpdateSceneViewMouseRay(SceneView sceneView) {
        Event e = Event.current;
        Vector3 mousePosition = e.mousePosition;

        // normal screenPointToRay doesn't work with scene view camera
        // world ray gets messed up when clicking mouse
        if (e.type is EventType.MouseMove or EventType.MouseDrag) {
            ptwr = HandleUtility.GUIPointToWorldRay(mousePosition);
        }
    }
}