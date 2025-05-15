using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareGrid : MonoBehaviour
{
    [SerializeField, Min(3)]
    private int size;

    [SerializeField]
    private Cell cellPrefab;

    private Cell[,] _cells;
    private CellInfo[,] cellInfo;

    public Cell this[int x, int y] {
        get {
            if (_cells == null) {
                _cells = new Cell[size, size];
            }
            return _cells[x, y];
        }
        set {
            if (_cells == null) {
                _cells = new Cell[size, size];
            }
            _cells[x, y] = value;
        }
    }

    public void GenerateGrid() {
        ClearCells();

        if (cellInfo == null) {
            cellInfo = new CellInfo[size, size];
        }

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                Cell cell = Instantiate(cellPrefab, transform);
                cell.transform.position = new Vector3(j, 0, i);
                cell.info = cellInfo[j, i];
                cell.SetCoords(i, j);
                cell.name = "Cell" + j + "," + i;
                cell.OnCellInfoChange += UpdateCellInfo;
                this[j, i] = cell;
            }
        }
    }

    private void UpdateCellInfo(CellInfo info, int i, int j) {
        cellInfo[j, i] = info;
    }

    public void ApplyCellInfo() {
        ForEachCell((cell, i, j) => cell.info = cellInfo[j, i]);
    }

    public void ClearCells() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Cell cell = transform.GetChild(i).GetComponent<Cell>();
            cell.OnCellInfoChange -= UpdateCellInfo;
            DestroyImmediate(cell.gameObject);
            cellInfo = null;
            _cells = null;
        }
    }

    private void ForEachCell(Action<Cell, int, int> action) {
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                action(this[j, i], i, j);
            }
        }
    }
}