using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Vectors;

#if UNITY_EDITOR
using UnityEditor;
#endif

[SelectionBase, ExecuteInEditMode]
public class SquareGrid : MonoBehaviour
{
    private VectorRenderer vr;

    [SerializeField, Min(3)]
    private int size;
    public int gridSize => size;

    private int currentSize;

    [SerializeField]
    private Cell cellPrefab;

    [SerializeField, HideInInspector]
    private Cell[] cells;

    // ordered list of the indexes of checkpoint cells in their checkpoint order
    private List<int> checkpointOrder;

    public Cell this[int x, int y] {
        get { return cells[y * size + x]; }
        set { cells[y * size + x] = value; }
    }

    public Cell this[Vector2Int v] {
        get { return cells[v.y * size + v.x]; }
        set { cells[v.y * size + v.x] = value; }
    }

    public IEnumerable<Cell> Cells() {
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                yield return this[x, y];
            }
        }
    }

    public bool sizeChanged => currentSize != size;

    private void OnEnable() {
        vr = GetComponent<VectorRenderer>();
#if UNITY_EDITOR
        EditorApplication.update += Update;
#endif
    }

    private void OnDisable() {
#if UNITY_EDITOR
        EditorApplication.update -= Update;
#endif
    }

    private void Update() {
        DrawPortalConnections();
    }

    public void GenerateGrid() {
        currentSize = size;
        ClearCells();

        cells = new Cell[size * size];
        checkpointOrder = new List<int>();

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                Cell cell = Instantiate(cellPrefab, transform);
                InitializeCell(ref cell, x, y);
                this[x, y] = cell;
            }
        }

        GetComponent<Pathfinder>()?.CreateGraph(this);
    }

    private void InitializeCell(ref Cell cell, int x, int y) {
        cell.transform.position = new Vector3(x, 0, y);
        cell.UpdateState();
        cell.SetCoords(x, y);
        cell.name = "Cell" + x + "," + y;
        cell.OnCellInfoChange += UpdateCellInfo;
    }

    private void UpdateCellInfo(CellInfo info, int x, int y) {
        CheckIfCheckPoint(info, x, y);

        this[x, y].info = info;

        UpdateCheckPointNumbers();

        GetComponent<Pathfinder>()?.CreateGraph(this);
    }

    private void CheckIfCheckPoint(CellInfo info, int x, int y) {
        Cell thisCell = this[x, y];
        bool wasThisCellCheckpoint = checkpointOrder.Contains(CellIndex(thisCell));
        int cellIndex = CellIndex(this[x, y]);
        if (info.checkpoint) {
            if (!wasThisCellCheckpoint) {
                // add it to the list
                checkpointOrder.Add(cellIndex);
            }
            else {
                // move it to the end of the list
                checkpointOrder.Remove(cellIndex);
                checkpointOrder.Add(cellIndex);
            }
        }
        else if (wasThisCellCheckpoint) {
            checkpointOrder.Remove(cellIndex);
        }
    }

    private void UpdateCheckPointNumbers() {
        for (int i = 0; i < checkpointOrder.Count; i++) {
            int checkpointIndex = checkpointOrder[i];
            Cell checkpointCell = cells[checkpointIndex];
            checkpointCell.SetCheckpointNumber((uint)i + 1);
        }
    }

    private void DrawPortalConnections() {
        List<Cell> portalCells = cells.Where(cell => cell.info.portal).ToList();
        using (vr.Begin()) {
            foreach (Cell cell in portalCells) {
                Vector3 start = cell.midpoint + Vector3.up * 0.6f;
                Vector3 end = this[cell.info.endPoint].midpoint;
                vr.Draw(start, end, Color.magenta, 0.07f, 0.23f);
            }
        }
    }

    public void ClearCells(bool nullilfyArrays = false) {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Cell cell = transform.GetChild(i).GetComponent<Cell>();
            cell.OnCellInfoChange -= UpdateCellInfo;
            DestroyImmediate(cell.gameObject);
            NullifyArrays();
        }
    }

    private void NullifyArrays() {
        cells = null;
        checkpointOrder = null;
    }

    private int CellIndex(Cell cell) {
        return Array.IndexOf(cells, cell);
    }
}