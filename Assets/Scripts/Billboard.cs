using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class Billboard : MonoBehaviour
{
    private void OnEnable() {
        EditorApplication.update += Update;
    }

    private void OnDisable() {
        EditorApplication.update -= Update;
    }

    void Update() {
        Camera editorCam = SceneView.lastActiveSceneView.camera;
        Vector3 camForward = editorCam.transform.forward;
        Vector3 flatForward = new Vector3(camForward.x, 0, camForward.z);
        transform.forward = flatForward.normalized;
    }
}