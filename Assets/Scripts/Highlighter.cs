using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class Highlighter : MonoBehaviour
{
    [SerializeField]
    private SquareGrid grid;
    private Camera editorCam;

    private void OnEnable() {
        EditorApplication.update += Update;
    }
    private void OnDisable() {
        EditorApplication.update -= Update;
    }

    void Update() {
        editorCam = SceneView.lastActiveSceneView.camera;
        Ray sptr = editorCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(sptr, out RaycastHit hitInfo, 1000, LayerMask.GetMask("Floor"))) {
            Vector3 gridSpaceCoords = hitInfo.point - grid.transform.position;
            Vector2 cellCoords = new Vector2(Mathf.Floor(gridSpaceCoords.x), Mathf.Floor(gridSpaceCoords.z));
        }
    }
}