using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[SelectionBase]
public class Cell : MonoBehaviour
{
    private bool coordsSet = false;
    private (int j, int i) coords;

    public void SetCoords(int i, int j) {
        if (!coordsSet) {
            coords = (j, i);
            coordsSet = true;
        }
    }

    [SerializeField]
    public CellInfo info;

    public event Action<CellInfo, int, int> OnCellInfoChange;
    public void CellInfoChanged() => OnCellInfoChange?.Invoke(info, coords.i, coords.j);
    
}

[Serializable]
public struct CellInfo
{
    [SerializeField]
    private bool blocked;

    private int byteState;

    public static CellInfo empty => new CellInfo();

    public void SetState(State state, bool value) {
        if (value) {
            byteState = byteState | (int)state;
        }
        else {
            byteState = byteState & ~(int)state;
        }
    }

    public void ClearState() {
        byteState = 0;
    }

    public enum State
    {
        Blocked = 0b_0001,
        Obstacle = 0b_0010,
        Portal = 0b_0100,
        Checkpoint = 0b_1000,
    }
}

[CustomPropertyDrawer(typeof(CellInfo))]
public class CellInfoDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        SerializedProperty blocked = property.FindPropertyRelative("blocked");
        GUIContent blockedLabel = new GUIContent("Blocked");

        EditorGUI.PropertyField(position, blocked, blockedLabel, true);
    }
}