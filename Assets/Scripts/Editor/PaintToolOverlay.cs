using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.UIElements;
using UnityEditor.Overlays;
using UnityEngine.UI;
using Toggle = UnityEngine.UIElements.Toggle;

[Overlay(typeof(SceneView), "Palette", false)]
public class PaintToolOverlayPanel : Overlay, ITransientOverlay
{
    private SquareGrid grid;
    public event Action<CellInfo.State, bool> OnSelectState;
    public event Action<int> OnObstacleWeightChange;
    public event Action<Vector2> OnEndPointChange;

    public void SetCurrentGrid(in SquareGrid currentGrid) {
        grid = currentGrid;
    }

    /// <inheritdoc />
    public bool visible => PaintTool.isActive;

    /// <inheritdoc />
    public override VisualElement CreatePanelContent() {
        VisualElement root = new VisualElement() { name = "Paint Tool Overlay Root" };

        CreateBlockedBox(ref root);
        CreateObstacleBox(ref root);
        CreatePortalBox(ref root);
        CreateCheckpointBox(ref root);

        return root;
    }

    private void CreateBlockedBox(ref VisualElement root) {
        Toggle toggleBlocked = new Toggle("Blocked");
        toggleBlocked.RegisterCallback<ChangeEvent<bool>>(context => {
            OnSelectState?.Invoke(CellInfo.State.Blocked, context.newValue);
        });
        VisualElement blockedBox = new Box();
        blockedBox.Add(toggleBlocked);
        root.Add(blockedBox);
    }

    private void CreateObstacleBox(ref VisualElement root) {
        VisualElement toggleObstacle = new Toggle("Obstacle");
        UnsignedIntegerField obstacleWeight = new UnsignedIntegerField();
        obstacleWeight.value = 1;
        obstacleWeight.selectAllOnFocus = true;

        obstacleWeight.RegisterCallback<ChangeEvent<uint>>(context => {
            if (context.newValue >= 1) {
                OnObstacleWeightChange?.Invoke((int)context.newValue);
            }
        });
        obstacleWeight.RegisterCallback<BlurEvent>(context => {
            obstacleWeight.value = Math.Max(1, obstacleWeight.value);
            obstacleWeight.MarkDirtyRepaint();
        });

        toggleObstacle.RegisterCallback<ChangeEvent<bool>>(context => {
            OnSelectState?.Invoke(CellInfo.State.Obstacle, context.newValue);
        });
        obstacleWeight.style.height = 16;
        VisualElement obstacleBox = new Box();
        obstacleBox.style.flexDirection = FlexDirection.Row;
        obstacleBox.Add(toggleObstacle);
        obstacleBox.Add(obstacleWeight);

        root.Add(obstacleBox);
    }

    private void CreatePortalBox(ref VisualElement root) {
        Toggle togglePortal = new Toggle("Portal");
        Vector2IntField portalEndpoint = new Vector2IntField();
        portalEndpoint.RegisterCallback<ChangeEvent<Vector2Int>>(context => {
            bool xInRange = context.newValue.x >= 0 && context.newValue.x < grid.gridSize;
            bool yInRange = context.newValue.y >= 0 && context.newValue.y < grid.gridSize;
            if (xInRange && yInRange) {
                OnEndPointChange?.Invoke(context.newValue);
            }
        });
        portalEndpoint.RegisterCallback<BlurEvent>(context => {
            int x = Math.Clamp(portalEndpoint.value.x, 0, grid.gridSize - 1);
            int y = Math.Clamp(portalEndpoint.value.y, 0, grid.gridSize - 1);
            portalEndpoint.value = new Vector2Int(x, y);
            portalEndpoint.MarkDirtyRepaint();
        });

        togglePortal.RegisterCallback<ChangeEvent<bool>>(context => {
            OnSelectState?.Invoke(CellInfo.State.Portal, context.newValue);
        });
        portalEndpoint.style.flexGrow = 1;
        portalEndpoint.style.height = 16;
        VisualElement portalBox = new Box();
        portalBox.style.flexDirection = FlexDirection.Row;
        portalBox.style.width = 240;
        portalBox.Add(togglePortal);
        portalBox.Add(portalEndpoint);

        root.Add(portalBox);
    }

    private void CreateCheckpointBox(ref VisualElement root) {
        Toggle toggleCheckpoint = new Toggle("Checkpoint");
        VisualElement checkpointBox = new Box();
        toggleCheckpoint.RegisterCallback<ChangeEvent<bool>>(context => {
            OnSelectState?.Invoke(CellInfo.State.Checkpoint, context.newValue);
        });
        checkpointBox.Add(toggleCheckpoint);

        root.Add(checkpointBox);
    }
}