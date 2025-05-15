using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[SelectionBase]
public class Cell : MonoBehaviour
{
    private IEnumerable<(int index, bool value)> GetStateIndices() {
        int state = info.state;
        for (int i = 0; i < 4; i++) {
            if ((state & 1) > 0) yield return (i, true);
            else yield return (i, false);
            state >>= 1;
        }
    }

    public void UpdateState() {
        foreach ((int index, bool value) in GetStateIndices()) {
            transform.GetChild(index + 1).gameObject.SetActive(value);
        }
    }

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
    public void CellInfoChanged() {
        OnCellInfoChange?.Invoke(info, coords.i, coords.j);
        UpdateState();
    }
}

[Serializable]
public struct CellInfo
{
    [SerializeField]
    private byte byteState;

    public static CellInfo empty => new CellInfo();

    public void SetState(State state, bool value) {
        if (value) {
            byteState |= (byte)state;
        }
        else {
            byteState &= (byte)(~state);
        }
    }

    public int state => (int)byteState;

    public void ClearState() {
        byteState = 0;
    }

    public enum State : byte
    {
        Unoccupied = 0,
        Blocked = 0b_0001,
        Obstacle = 0b_0010,
        Portal = 0b_0100,
        Checkpoint = 0b_1000,
    }

    /// <inheritdoc />
    public override string ToString() {
        return Convert.ToString(byteState, 2).PadLeft(4, '0');
    }
}

[CustomPropertyDrawer(typeof(CellInfo))]
public class CellInfoDrawer : PropertyDrawer
{
    GUIContent blockedLabel = new GUIContent("Blocked");
    GUIContent obstacleLabel = new GUIContent("Obstacle");
    GUIContent portalLabel = new GUIContent("Portal");
    GUIContent checkpointLabel = new GUIContent("Checkpoint");

    // public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    //     return EditorGUI.GetPropertyHeight(property, label, true);
    // }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        GUIStyle propertyTitle = new GUIStyle();
        propertyTitle.fontStyle = FontStyle.Bold;
        propertyTitle.normal.textColor = Color.white;

        EditorGUILayout.LabelField("State", propertyTitle);

        SerializedProperty byteStateProperty = property.FindPropertyRelative("byteState");
        byte byteState = (byte)byteStateProperty.intValue;
        bool blocked = (byteState & (byte)CellInfo.State.Blocked) > 0;
        bool obstacle = (byteState & (byte)CellInfo.State.Obstacle) > 0;
        bool portal = (byteState & (byte)CellInfo.State.Portal) > 0;
        bool checkpoint = (byteState & (byte)CellInfo.State.Checkpoint) > 0;

        blocked = EditorGUILayout.Toggle(blockedLabel, blocked);
        obstacle = EditorGUILayout.Toggle(obstacleLabel, obstacle);
        portal = EditorGUILayout.Toggle(portalLabel, portal);
        checkpoint = EditorGUILayout.Toggle(checkpointLabel, checkpoint);

        byte newByteState = 0;
        if (blocked) newByteState |= (byte)CellInfo.State.Blocked;
        if (obstacle) newByteState |= (byte)CellInfo.State.Obstacle;
        if (portal) newByteState |= (byte)CellInfo.State.Portal;
        if (checkpoint) newByteState |= (byte)CellInfo.State.Checkpoint;
        byteStateProperty.intValue = newByteState;
    }
}