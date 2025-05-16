using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Billboard : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnEnable() {
        EditorApplication.update += Update;
    }

    private void OnDisable() {
        EditorApplication.update -= Update;
    }

    void Update() {
        if (!GetSceneViewCamera(out Camera sceneViewCamera)) return;
        Vector3 toBillboard = (transform.position - sceneViewCamera.transform.position).normalized;
        Vector3 flatForward = new Vector3(toBillboard.x, 0, toBillboard.z).normalized;
        transform.forward = flatForward.normalized;
    }

    public static bool GetSceneViewCamera(out Camera sceneCamera) {
        sceneCamera = null;
        SceneView sceneView = SceneView.sceneViews.Count > 0 ? (SceneView)SceneView.sceneViews[0] : null;
        if (!sceneView) return false;
        sceneCamera = sceneView.camera;
        return true;
    }
#endif
}