using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

[EditorTool("Paint", typeof(Highlighter))]
public class PaintTool : EditorTool
{
    [Shortcut("Activate Paint Tool", KeyCode.U)]
    static void PathManipulatorToolShortcut() {
        if (Selection.GetFiltered<Highlighter>(SelectionMode.TopLevel).Length > 0) {
            ToolManager.SetActiveTool<PaintTool>();
        }
    }

    public override void OnToolGUI(EditorWindow window) {
        if (!(window is SceneView)) return;

        // https://discussions.unity.com/t/prevent-custom-editortool-from-being-deselected-with-mouse-button-clicks/929028
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        foreach (var obj in targets) {
            if (!(obj is Highlighter highlighter)) continue;
            if (!highlighter.currentCell) continue;

            highlighter.ShowMarker();

            if (EditorGUI.EndChangeCheck()) {
                // Undo.RecordObject(highlighter, "Interpolator");
            }
        }
    }
}