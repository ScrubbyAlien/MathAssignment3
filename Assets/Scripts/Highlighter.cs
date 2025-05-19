using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[ExecuteAlways]
public class Highlighter : MonoBehaviour
{
    [SerializeField]
    private SquareGrid _grid;
    public SquareGrid grid => _grid;

    [SerializeField]
    private Transform marker;

    public Cell currentCell { get; private set; }

    public bool mouseInsideSceneView { get; private set; }
    public bool markerOnGrid { get; set; }
    private Ray ptwr;

    private void OnEnable() {
        SceneView.duringSceneGui += UpdateSceneViewMouseRay;
        EditorApplication.update += Update;
    }
    private void OnDisable() {
        SceneView.duringSceneGui -= UpdateSceneViewMouseRay;
        EditorApplication.update -= Update;
    }

    private void Update() {
        currentCell = null;
        if (Physics.Raycast(ptwr, out RaycastHit hitInfo, 1000, LayerMask.GetMask("Floor"))) {
            markerOnGrid = true;

            Vector3 gridSpaceCoords = hitInfo.point - grid.transform.position;
            Vector2 cellCoords = new Vector2(Mathf.Floor(gridSpaceCoords.x), Mathf.Floor(gridSpaceCoords.z));
            currentCell = grid[(int)cellCoords.x, (int)cellCoords.y];

            // add center offset plus zbuffer artifact prevention
            Vector3 newPosition = currentCell.transform.position + new Vector3(0.5f, 0.001f, 0.5f);
            // Vector3 newPosition = gridSpaceCoords + new Vector3(0, 0.001f, 0);
            marker.position = newPosition;
        }
        else {
            markerOnGrid = false;
            HideMarker();
        }
    }

    public void ShowMarker() => marker.gameObject.SetActive(true);
    public void HideMarker() => marker.gameObject.SetActive(false);

    public void SetMarkerPosition(Vector3 position) => marker.position = position;

    // https://discussions.unity.com/t/tracking-the-mouse-position-in-the-editor/7124/2
    private void UpdateSceneViewMouseRay(SceneView sceneView) {
        mouseInsideSceneView = true;
        Event e = Event.current;
        Vector3 mousePosition = e.mousePosition;

        if (mousePosition.x > sceneView.cameraViewport.width || mousePosition.x < 0f) {
            mouseInsideSceneView = false;
        }
        if (mousePosition.y > sceneView.cameraViewport.height || mousePosition.y < 0f) {
            mouseInsideSceneView = false;
        }

        // normal screenPointToRay doesn't work with scene view camera
        // world ray gets messed up when clicking mouse
        if (e.type is EventType.MouseMove or EventType.MouseDrag) {
            ptwr = HandleUtility.GUIPointToWorldRay(mousePosition);
        }
    }
}