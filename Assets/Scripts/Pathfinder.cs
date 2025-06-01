using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    private SquareGrid grid;
    [SerializeField]
    private Vector2Int startPosition;
    [SerializeField, Min(0)]
    private int stepsPerRound;

    private WeightedDigraph<CellInfo> graph;
    private List<int> nodeIndicies;

    private void OnValidate() {
        if (!grid) return;
        Vector2Int clampedStartPosition = startPosition;
        clampedStartPosition.x = Math.Clamp(clampedStartPosition.x, 0, grid.gridSize - 1);
        clampedStartPosition.y = Math.Clamp(clampedStartPosition.y, 0, grid.gridSize - 1);
        startPosition = clampedStartPosition;
    }

    public void CreateGraph(SquareGrid fromGrid) {
        grid = fromGrid;

        graph = new();
        nodeIndicies = new List<int>();

        // add nodes
        foreach (Cell cell in grid.Cells()) {
            int nodeIndex = graph.AddNode(cell.info);
            nodeIndicies.Add(nodeIndex);
        }

        // add edges
        foreach (int index in nodeIndicies) {
            List<int> adjacent = GetAdjacentIndices(index);
            CellInfo cellInfo = graph[index].ReadData();
            if (cellInfo.blocked) continue; // don't form any edges with blocked cells

            uint weight = 1 + cellInfo.obstacleWeight;
            foreach (int adjacentIndex in adjacent) {
                if (graph[adjacentIndex].ReadData().blocked) continue;
                graph.AddEdge(index, adjacentIndex, weight);
            }

            // form edge weight 1 edge with portal end point if it is not blocked
            if (cellInfo.portal) {
                int endPointIndex = cellInfo.endPoint.y * grid.gridSize + cellInfo.endPoint.x;
                CellInfo endPointInfo = graph[endPointIndex].ReadData();
                if (!endPointInfo.blocked) graph.AddEdge(index, endPointIndex, 1);
            }
        }
    }

    private List<int> GetAdjacentIndices(int index) {
        List<int> indices = new List<int>();
        int left = index - 1;
        int right = index + 1;
        int up = index + grid.gridSize;
        int down = index - grid.gridSize;

        if (index % grid.gridSize < grid.gridSize - 1) { // cell is not on rightmost column
            indices.Add(right);
        }
        if (index % grid.gridSize > 0)  { // cell is not on leftmost column
            indices.Add(left);
        }
        if (index > grid.gridSize) { // cell is not on bottom row
            indices.Add(down);
        }
        if (index < grid.gridSize * grid.gridSize - grid.gridSize) { // cell is not on top row
            indices.Add(up);
        }

        return indices;
    }

    private bool IndexInBounds(int index) {
        return index >= 0 && index < grid.gridSize;
    }
}