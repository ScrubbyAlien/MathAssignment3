using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEditor;

[SelectionBase]
public class Cell : MonoBehaviour
{
    [SerializeField]
    public CellInfo info;
    [SerializeField]
    private GameObject[] stateObjects;
    [SerializeField]
    private Gradient obstacleGradient;

    [SerializeField]
    private bool coordsSet = false;
    public Vector2Int coords { get; private set; }

    public Vector3 midpoint => transform.position + new Vector3(0.5f, 0, 0.5f);

    [HideInInspector]
    public int nodeIndex;

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
            GameObject stateObject = stateObjects[index].gameObject;
            stateObject.SetActive(value);
            if (index == 1 && value) { // set obstacle color
                float time = info.obstacleWeight / 20f;
                stateObject.GetComponent<SpriteRenderer>().color = obstacleGradient.Evaluate(time);
            }
        }
    }

    public void SetCoords(int x, int y) {
        if (!coordsSet) {
            coords = new Vector2Int(x, y);
            coordsSet = true;
        }
    }

    public event Action<CellInfo, int, int> OnCellInfoChange;
    public void CellInfoChanged() {
        OnCellInfoChange?.Invoke(info, coords.x, coords.y);
        UpdateState();
    }

    public void SetCheckpointNumber(uint number) {
        stateObjects[3].GetComponentInChildren<TextMesh>().text = number.ToString();
        info.checkpointOrder = number;
    }
}

[Serializable]
public struct CellInfo
{
    [SerializeField]
    private int _state;
    [SerializeField]
    private uint _obstacleWeight;
    [SerializeField]
    private Vector2Int _endPoint;
    [SerializeField]
    private uint _checkpointOrder;

    public int state => _state;
    public uint obstacleWeight {
        get {
            if (InState(State.Obstacle)) return _obstacleWeight;
            else return 0;
        }
        set {
            if (value < 1) {
                _obstacleWeight = 1;
            }
            else {
                _obstacleWeight = value;
            }
        }
    }
    public Vector2Int endPoint {
        get => _endPoint;
        set => _endPoint = value;
    }
    public uint checkpointOrder {
        get => _checkpointOrder;
        set => _checkpointOrder = value;
    }

    public static CellInfo empty => new CellInfo();

    public void SetState(State newState, bool value) {
        if (value) _state |= (int)newState;
        else _state &= ~(int)newState;
    }

    public void SetState(int newState) {
        _state = newState;
    }

    public bool InState(State queryState) {
        return (_state & (int)queryState) > 0;
    }

    public bool blocked => InState(State.Blocked);
    public bool obstacle => InState(State.Obstacle);
    public bool portal => InState(State.Portal);
    public bool checkpoint => InState(State.Checkpoint);

    public void ClearState() {
        _state = 0;
    }

    public enum State : int
    {
        Unoccupied = 0,
        Blocked = 0b_0001,
        Obstacle = 0b_0010,
        Portal = 0b_0100,
        Checkpoint = 0b_1000,
    }

    /// <inheritdoc />
    public override string ToString() {
        string state = Convert.ToString(_state, 2).PadLeft(4, '0');
        string data = $"ow: {obstacleWeight}, ep: {endPoint}, co: {checkpointOrder}";
        return $"{state} ({data})";
    }
}

[CustomPropertyDrawer(typeof(CellInfo))]
public class CellInfoDrawer : PropertyDrawer
{
    GUIContent blockedLabel = new GUIContent("Blocked");
    GUIContent obstacleLabel = new GUIContent("Obstacle");
    GUIContent portalLabel = new GUIContent("Portal");
    GUIContent checkpointLabel = new GUIContent("Checkpoint");

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, label, true) - EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        SquareGrid grid = GameObject.FindObjectOfType<SquareGrid>();

        GUIStyle propertyTitle = new GUIStyle();
        propertyTitle.fontStyle = FontStyle.Bold;
        propertyTitle.normal.textColor = Color.white;

        EditorGUILayout.LabelField("State", propertyTitle);

        SerializedProperty stateProperty = property.FindPropertyRelative("_state");
        int state = stateProperty.intValue;
        bool blocked = (state & (int)CellInfo.State.Blocked) > 0;
        bool obstacle = (state & (int)CellInfo.State.Obstacle) > 0;
        bool portal = (state & (int)CellInfo.State.Portal) > 0;
        bool checkpoint = (state & (int)CellInfo.State.Checkpoint) > 0;

        blocked = EditorGUILayout.Toggle(blockedLabel, blocked);
        obstacle = EditorGUILayout.Toggle(obstacleLabel, obstacle);
        portal = EditorGUILayout.Toggle(portalLabel, portal);
        EditorGUILayout.BeginHorizontal();
        checkpoint = EditorGUILayout.Toggle(checkpointLabel, checkpoint);
        EditorGUILayout.LabelField($"Order: {property.FindPropertyRelative("_checkpointOrder").uintValue}");
        EditorGUILayout.EndHorizontal();

        byte newByteState = 0;
        if (blocked) newByteState |= (int)CellInfo.State.Blocked;
        if (obstacle) newByteState |= (int)CellInfo.State.Obstacle;
        if (portal) newByteState |= (int)CellInfo.State.Portal;
        if (checkpoint) newByteState |= (int)CellInfo.State.Checkpoint;
        stateProperty.intValue = newByteState;

        if (obstacle || portal) {
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Cell Data", propertyTitle);

            SerializedProperty obstacleWeight = property.FindPropertyRelative("_obstacleWeight");
            SerializedProperty endPoint = property.FindPropertyRelative("_endPoint");

            if (obstacle) EditorGUILayout.PropertyField(obstacleWeight);
            if (portal) EditorGUILayout.PropertyField(endPoint);

            obstacleWeight.uintValue = Math.Max(1, obstacleWeight.uintValue);

            Vector2Int clampedEndPointCoords = endPoint.vector2IntValue;
            clampedEndPointCoords.x = Math.Clamp(clampedEndPointCoords.x, 0, grid.gridSize - 1);
            clampedEndPointCoords.y = Math.Clamp(clampedEndPointCoords.y, 0, grid.gridSize - 1);
            endPoint.vector2IntValue = clampedEndPointCoords;
        }
    }
}