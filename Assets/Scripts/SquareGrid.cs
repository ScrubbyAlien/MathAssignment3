using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareGrid : MonoBehaviour
{
    [SerializeField, Min(3)]
    private int size;

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

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                Cell cell = Instantiate(cellPrefab, transform);
                InitializeCell(ref cell, i, j);
                this[j, i] = cell;
            }
        }
    }

    public void HighlightCell(Vector2 coords) {
        Debug.Log(coords);
    }

    private void InitializeCell(ref Cell cell, int i, int j) {
        cell.transform.position = new Vector3(j, 0, i);
        cell.info = cellInfo[j * size + i];
        cell.UpdateState();
        cell.SetCoords(i, j);
        cell.name = "Cell" + j + "," + i;
        cell.OnCellInfoChange += UpdateCellInfo;
    }

    private void UpdateCellInfo(CellInfo info, int i, int j) {
        cellInfo[j * size + i] = info;
    }

    public void ApplyCellInfo() {
        ForEachCell((cell, i, j) => {
            cell.info = cellInfo[j * size + i];
            cell.UpdateState();
        });
    }

    public void ClearCells(bool nullilfyArrays = false) {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Cell cell = transform.GetChild(i).GetComponent<Cell>();
            cell.OnCellInfoChange -= UpdateCellInfo;
            DestroyImmediate(cell.gameObject);
            if (nullilfyArrays) NullifyArrays();
        }
    }

    private void ForEachCell(Action<Cell, int, int> action) {
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                action(this[j, i], i, j);
            }
        }
    }

    private void NullifyArrays() {
        cellInfo = null;
        _cells = null;
    }
}