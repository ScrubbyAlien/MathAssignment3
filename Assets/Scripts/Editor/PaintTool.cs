using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ShortcutManagement;
using UnityEngine;

[EditorTool("Palette", typeof(SquareGrid)), Icon("Assets/Icon/art-palette.png")]
public class PaintTool : EditorTool
{
    public static bool isActive { get; private set; }

    private bool mouseDown;
    private Ray ptwr;
    private PaintToolOverlayPanel palette;

    private int currentState;
    private uint currentObstacleWeight = 1;
    private Vector2Int currentEndPoint;

    [Shortcut("Activate Paint Tool", KeyCode.U)]
    static void PaintToolShortcut() {
        if (Selection.GetFiltered<SquareGrid>(SelectionMode.TopLevel).Length > 0) {
            ToolManager.SetActiveTool<PaintTool>();
        }
    }

    public override void OnToolGUI(EditorWindow window) {
        if (!(window is SceneView)) return;

        // https://discussions.unity.com/t/prevent-custom-editortool-from-being-deselected-with-mouse-button-clicks/929028
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        foreach (var obj in targets) {
            if (!(obj is SquareGrid grid)) continue;
            palette.SetGrid(grid);
            if (currentObstacleWeight < 1) currentObstacleWeight = 1;
            if (!HighlightCurrentCell(grid, out Cell currentCell)) continue;

            if (mouseDown) {
                CellInfo newCellInfo = new CellInfo();
                newCellInfo.SetState(currentState);
                newCellInfo.obstacleWeight = currentObstacleWeight;
                newCellInfo.endPoint = currentEndPoint;
                currentCell.info = newCellInfo;
                currentCell.CellInfoChanged();
            }

            if (EditorGUI.EndChangeCheck()) { }
        }
    }

    /// <inheritdoc />
    public override void OnActivated() {
        base.OnActivated();
        ResetPalette();

        SceneView.duringSceneGui += UpdateSceneViewMouseRay;

        SceneView.lastActiveSceneView.TryGetOverlay("Palette", out Overlay match);
        if (match is PaintToolOverlayPanel) {
            palette = match as PaintToolOverlayPanel;
            palette.OnSelectState += PaletteStateChanged;
            palette.OnObstacleWeightChange += PaletteObstacleWeightChanged;
            palette.OnEndPointChange += PaletteEndPointChanged;
            palette.Initialize(currentState, currentObstacleWeight, currentEndPoint);
        }

        isActive = true;
    }

    /// <inheritdoc />
    public override void OnWillBeDeactivated() {
        base.OnWillBeDeactivated();
        SceneView.duringSceneGui -= UpdateSceneViewMouseRay;

        if (palette != null) {
            palette.OnSelectState -= PaletteStateChanged;
            palette.OnObstacleWeightChange -= PaletteObstacleWeightChanged;
            palette.OnEndPointChange -= PaletteEndPointChanged;
        }

        isActive = false;
    }

    private void PaletteStateChanged(CellInfo.State state, bool value) {
        if (value) currentState |= (int)state; // turn on state
        else currentState &= ~(int)state; // turn off state;
    }

    private void PaletteObstacleWeightChanged(uint obstacleWeight) {
        currentObstacleWeight = obstacleWeight;
    }

    private void PaletteEndPointChanged(Vector2Int endPoint) {
        currentEndPoint = endPoint;
    }

    private bool HighlightCurrentCell(SquareGrid grid, out Cell currentCell) {
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
        // world ray gets messed up when clicking mouse so we only update ptwr
        // on an event where the mouse moves
        if (e.type is EventType.MouseMove or EventType.MouseDrag) {
            ptwr = HandleUtility.GUIPointToWorldRay(mousePosition);
        }
        if (e.type is EventType.MouseDown or EventType.MouseDrag) {
            mouseDown = e.button == 0;
        }
        else {
            mouseDown = false;
        }
    }

    private void ResetPalette() {
        currentState = 0;
        currentEndPoint = Vector2Int.zero;
        currentObstacleWeight = 1;
    }
}