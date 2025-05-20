using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.UIElements;
using UnityEditor.Overlays;

[Overlay(typeof(SceneView), "Palette", false)]
public class PaintToolOverlayPanel : Overlay, ITransientOverlay
{
    public event Action<int, bool, int, Vector2> OnSelectState;

    /// <inheritdoc />
    public override VisualElement CreatePanelContent() {
        VisualElement root = new VisualElement() { name = "Paint Tool Overlay Root" };

        // BLOCKED
        VisualElement toggleBlocked = new Toggle("Blocked");
        VisualElement blockedBox = new Box();
        blockedBox.Add(toggleBlocked);

        // OBSTACLE
        VisualElement toggleObstacle = new Toggle("Obstacle");
        VisualElement obstacleWeight = new IntegerField();
        obstacleWeight.style.height = 16;
        VisualElement obstacleBox = new Box();
        obstacleBox.style.flexDirection = FlexDirection.Row;
        obstacleBox.Add(toggleObstacle);
        obstacleBox.Add(obstacleWeight);

        // PORTAL
        VisualElement togglePortal = new Toggle("Portal");
        VisualElement portalEndpoint = new Vector2Field();
        portalEndpoint.style.flexGrow = 1;
        portalEndpoint.style.height = 16;
        VisualElement portalBox = new Box();
        portalBox.style.flexDirection = FlexDirection.Row;
        portalBox.style.width = 240;
        portalBox.Add(togglePortal);
        portalBox.Add(portalEndpoint);

        // CHECKPOINT
        VisualElement toggleCheckpoint = new Toggle("Checkpoint");
        VisualElement checkpointBox = new Box();
        checkpointBox.Add(toggleCheckpoint);

        root.Add(blockedBox);
        root.Add(obstacleBox);
        root.Add(portalBox);
        root.Add(checkpointBox);

        return root;
    }

    /// <inheritdoc />
    public bool visible {
        get { return PaintTool.isActive; }
    }
}