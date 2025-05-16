using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class Highlighter : MonoBehaviour
{
    [SerializeField]
    private SquareGrid _grid;
    public SquareGrid grid => _grid;

    public Cell currentCell { get; private set; }

    public bool mouseInsideSceneView { get; private set; }
    private Ray ptwr;

    private void OnEnable() {
        SceneView.duringSceneGui += UpdateSceneViewMousePosition;
        EditorApplication.update += Update;
    }
    private void OnDisable() {
        SceneView.duringSceneGui -= UpdateSceneViewMousePosition;
        EditorApplication.update -= Update;
    }

    private void Update() {
        if (currentCell) currentCell.Unhighlight();
        currentCell = null;
        if (Physics.Raycast(ptwr, out RaycastHit hitInfo, 1000, LayerMask.GetMask("Floor"))) {
            Vector3 gridSpaceCoords = hitInfo.point - grid.transform.position;
            Vector2 cellCoords = new Vector2(Mathf.Floor(gridSpaceCoords.x), Mathf.Floor(gridSpaceCoords.z));
            currentCell = grid[(int)cellCoords.x, (int)cellCoords.y];
        }
    }

    // https://discussions.unity.com/t/tracking-the-mouse-position-in-the-editor/7124/2
    private void UpdateSceneViewMousePosition(SceneView sceneView) {
        mouseInsideSceneView = true;
        Vector3 mousePosition = Event.current.mousePosition;

        if (mousePosition.x > sceneView.cameraViewport.width || mousePosition.x < 0f) {
            mouseInsideSceneView = false;
        }
        if (mousePosition.y > sceneView.cameraViewport.height || mousePosition.y < 0f) {
            mouseInsideSceneView = false;
        }

        // normal screenPointToRay doesn't work with scene view camera
        ptwr = HandleUtility.GUIPointToWorldRay(mousePosition);
    }
}