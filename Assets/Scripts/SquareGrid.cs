using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[SelectionBase]
public class SquareGrid : MonoBehaviour
{
    [SerializeField, Min(3)]
    private int size;
    public int gridSize => size;

    private int currentSize;

    [SerializeField]
    private Cell cellPrefab;

    [SerializeField, HideInInspector]
    private Cell[] _cells;
    [SerializeField, HideInInspector]
    private CellInfo[] cellInfo;

    public Cell this[int x, int y] {
        get {
            if (_cells == null) {
                _cells = new Cell[size * size];
            }
            return _cells[y * size + x];
        }
        set {
            if (_cells == null) {
                _cells = new Cell[size * size];
            }
            _cells[y * size + x] = value;
        }
    }

    public IEnumerable<Cell> Cells() {
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                yield return this[x, y];
            }
        }
    }

    public bool sizeChanged => currentSize != size;

    public void GenerateGrid() {
        if (size != currentSize) {
            ClearCells(true);
            currentSize = size;
        }
        else {
            ClearCells();
        }

        if (cellInfo == null) {
            cellInfo = new CellInfo[size * size];
        }

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
        cell.info = cellInfo[y * size + x];
        cell.UpdateState();
        cell.SetCoords(x, y);
        cell.name = "Cell" + x + "," + y;
        cell.OnCellInfoChange += UpdateCellInfo;
        // disable picking this object in the scene view
        // SceneVisibilityManager.instance.DisablePicking(cell.gameObject, true);
    }

    private void UpdateCellInfo(CellInfo info, int x, int y) {
        cellInfo[y * size + x] = info;
        this[x, y].info = info;
        GetComponent<Pathfinder>()?.CreateGraph(this);
    }

    public void ClearCells(bool nullilfyArrays = false) {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Cell cell = transform.GetChild(i).GetComponent<Cell>();
            cell.OnCellInfoChange -= UpdateCellInfo;
            DestroyImmediate(cell.gameObject);
            if (nullilfyArrays) NullifyArrays();
        }
    }

    private void NullifyArrays() {
        cellInfo = null;
        _cells = null;
    }
}