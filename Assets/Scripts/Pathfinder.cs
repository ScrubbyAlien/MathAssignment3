using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Vectors;

[ExecuteAlways]
public class Pathfinder : MonoBehaviour
{
    private VectorRenderer vr;
    private SquareGrid grid;
    [SerializeField]
    private Vector2Int startPosition;
    [SerializeField, Min(0)]
    private int stepsPerRound;
    [SerializeField]
    private bool showAllPaths = true;
    [SerializeField]
    private bool canMoveDiagonally = false;

    private WeightedDigraph<Cell> graph;
    private List<int> nodeIndicies;

    private const float vectorRadius = 0.125f;
    private const float vectorTipHeight = 0.25f;
    private const float doubleDrawOffset = 0.3f;

    private Path<Cell> pathThroughCheckpoints;
    private List<Path<Cell>> pathsToCheckpoints;
    private Dictionary<Node<Cell>, Path<Cell>> allPaths;

    private void OnEnable() {
        vr = GetComponent<VectorRenderer>();
    }

    private void OnValidate() {
        if (!grid) return;
        Vector2Int clampedStartPosition = startPosition;
        clampedStartPosition.x = Math.Clamp(clampedStartPosition.x, 0, grid.gridSize - 1);
        clampedStartPosition.y = Math.Clamp(clampedStartPosition.y, 0, grid.gridSize - 1);
        startPosition = clampedStartPosition;

        CreateGraph(grid);
    }

    private void Update() {
        DrawVectors();
    }

    private void DrawVectors() {
        if (!grid || !grid.PositionInGrid(startPosition)) return;

        Vector3 startpositionStart = grid[startPosition].midpoint + Vector3.up * 1.5f;
        Vector3 startpositionEnd = startpositionStart + Vector3.down * 1.3f;
        using (vr.Begin()) {
            vr.Draw(startpositionStart, startpositionEnd, Color.green, 0.2f, 0.43f);
        }

        if (graph == null) return;

        HashSet<Edge<Cell>> blueEdges = new();
        HashSet<Edge<Cell>> greenEdges = new();
        HashSet<Edge<Cell>> greyEdges = new();

        // draw shortest path to each checkpoint from start
        foreach (Path<Cell> path in pathsToCheckpoints) {
            using (vr.Begin()) {
                foreach (Edge<Cell> edge in path.edges) {
                    Vector3 start = edge.start.ReadData().midpoint;
                    Vector3 end = edge.end.ReadData().midpoint;
                    Edge<Cell> reverseEdge = graph.GetEdge(edge.end, edge.start);
                    if (blueEdges.Contains(reverseEdge)) {
                        start += Vector3.up * doubleDrawOffset;
                        end += Vector3.up * doubleDrawOffset;
                    }
                    vr.Draw(start, end, Color.blue, vectorRadius, vectorTipHeight);
                    blueEdges.Add(edge);
                }
            }
        }

        if (pathsToCheckpoints.Count > 1) {
            // draw path through checkpoints
            using (vr.Begin()) {
                foreach (Edge<Cell> edge in pathThroughCheckpoints.edges) {
                    Vector3 start = edge.start.ReadData().midpoint;
                    Vector3 end = edge.end.ReadData().midpoint;
                    Edge<Cell> reverseEdge = graph.GetEdge(edge.end, edge.start);
                    if (blueEdges.Contains(edge) || blueEdges.Contains(reverseEdge)) {
                        start += Vector3.up * doubleDrawOffset;
                        end += Vector3.up * doubleDrawOffset;
                    }
                    if (greenEdges.Contains(reverseEdge)) {
                        start += Vector3.up * doubleDrawOffset;
                        end += Vector3.up * doubleDrawOffset;
                    }
                    vr.Draw(start, end, Color.green, vectorRadius, vectorTipHeight);
                    greenEdges.Add(edge);
                }
            }
        }

        // draw paths to all points that are reachable within one round
        if (showAllPaths) {
            foreach (Path<Cell> path in allPaths.Values) {
                using (vr.Begin()) {
                    foreach (Edge<Cell> edge in path.edges) {
                        // don't draw arrows ontop of each other
                        Edge<Cell> reverseEdge = graph.GetEdge(edge.end, edge.start);
                        if (blueEdges.Contains(edge) || blueEdges.Contains(reverseEdge)) continue;
                        if (greenEdges.Contains(edge) || greenEdges.Contains(reverseEdge)) continue;
                        if (greyEdges.Contains(edge) || greyEdges.Contains(reverseEdge)) continue;
                        Vector3 start = edge.start.ReadData().midpoint;
                        Vector3 end = edge.end.ReadData().midpoint;
                        vr.Draw(start, end, Color.gray, vectorRadius, vectorTipHeight);
                        greyEdges.Add(edge);
                    }
                }
            }
        }
    }

    public void CreateGraph(SquareGrid fromGrid) {
        grid = fromGrid;

        graph = new();
        nodeIndicies = new List<int>();

        // add nodes
        foreach (Cell cell in grid.Cells()) {
            int nodeIndex = graph.AddNode(cell);
            cell.nodeIndex = nodeIndex;
            nodeIndicies.Add(nodeIndex);
        }

        // add edges
        foreach (int index in nodeIndicies) {
            List<int> adjacent = GetAdjacentIndices(index);
            CellInfo cellInfo = graph[index].ReadData().info;
            if (cellInfo.blocked) continue; // don't form any edges with blocked cells

            uint weight = 1 + cellInfo.obstacleWeight;
            for (int i = 0; i < adjacent.Count; i++) {
                int adjacentIndex = adjacent[i];
                if (graph[adjacentIndex].ReadData().info.blocked) continue;
                graph.AddEdge(index, adjacentIndex, weight);
            }

            // form edge weight 1 edge with portal end point if it is not blocked
            if (cellInfo.portal) {
                int endPointIndex = cellInfo.endPoint.y * grid.gridSize + cellInfo.endPoint.x;
                CellInfo endPointInfo = graph[endPointIndex].ReadData().info;
                if (!endPointInfo.blocked) graph.AddEdge(index, endPointIndex, 1);
            }
        }

        if (!grid || !grid.PositionInGrid(startPosition)) return;

        // calculate paths
        Node<Cell> startNode = graph.GetNodeByIndex(grid[startPosition].nodeIndex);
        Node<Cell>[] checkpointNodes = grid.checkpointNodeIndicies
                                           .Where(i => !grid[i].info.blocked)
                                           .Select(i => graph.GetNodeByIndex(i))
                                           .ToArray();

        pathThroughCheckpoints = graph.Dijkstra(startNode, checkpointNodes).Subpath(stepsPerRound);
        graph.Dijkstra(startNode, checkpointNodes, out List<Path<Cell>> toCheckpoints, int.MaxValue);
        pathsToCheckpoints = toCheckpoints.Select(p => p.Subpath(stepsPerRound)).ToList();
        allPaths = graph.Dijkstra(startNode, stepsPerRound);

        // update checkpoint cost numbers
        foreach (int index in grid.checkpointNodeIndicies) {
            Node<Cell> node = graph[index];
            allPaths.TryGetValue(node, out Path<Cell> path);
            if (path != null && path.totalCost <= stepsPerRound) {
                node.ReadData().SetCostNumber((int)path.totalCost);
            }
            else node.ReadData().SetCostNumber(-1);
        }
    }

    private List<int> GetAdjacentIndices(int index) {
        List<int> indices = new List<int>();
        int left = index - 1;
        int right = index + 1;
        int up = index + grid.gridSize;
        int down = index - grid.gridSize;

        bool notRightMostColumn = index % grid.gridSize < grid.gridSize - 1;
        bool notBottomRow = index > grid.gridSize;
        bool notLeftMostColumn = index % grid.gridSize > 0;
        bool notTopRow = index < grid.gridSize * grid.gridSize - grid.gridSize;

        if (notRightMostColumn) indices.Add(right);
        if (notBottomRow) indices.Add(down);
        if (notLeftMostColumn) indices.Add(left);
        if (notTopRow) indices.Add(up);

        if (canMoveDiagonally) {
            int downRight = right - grid.gridSize;
            int downLeft = left - grid.gridSize;
            int upLeft = left + grid.gridSize;
            int upRight = right + grid.gridSize;

            if (notRightMostColumn && notBottomRow) indices.Add(downRight);
            if (notLeftMostColumn && notBottomRow) indices.Add(downLeft);
            if (notLeftMostColumn && notTopRow) indices.Add(upLeft);
            if (notRightMostColumn && notTopRow) indices.Add(upRight);
        }

        return indices;
    }

    private bool IndexInBounds(int index) {
        return index >= 0 && index < grid.gridSize;
    }
}