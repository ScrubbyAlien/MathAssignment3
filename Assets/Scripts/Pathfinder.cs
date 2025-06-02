using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public bool showAllPaths = true;

    private WeightedDigraph<Cell> graph;
    private List<int> nodeIndicies;

    private const float vectorRadius = 0.125f;
    private const float vectorTipHeight = 0.25f;

    private void OnEnable() {
        vr = GetComponent<VectorRenderer>();
    }

    private void OnValidate() {
        if (!grid) return;
        Vector2Int clampedStartPosition = startPosition;
        clampedStartPosition.x = Math.Clamp(clampedStartPosition.x, 0, grid.gridSize - 1);
        clampedStartPosition.y = Math.Clamp(clampedStartPosition.y, 0, grid.gridSize - 1);
        startPosition = clampedStartPosition;
    }

    private void Update() {
        DrawVectors();
    }

    private void DrawVectors() {
        Vector3 startpositionStart = grid[startPosition].midpoint + Vector3.up * 1.5f;
        Vector3 startpositionEnd = startpositionStart + Vector3.down * 1.3f;
        using (vr.Begin()) {
            vr.Draw(startpositionStart, startpositionEnd, Color.green, 0.2f, 0.43f);
        }

        if (graph != null) {
            Node<Cell> startNode = graph.GetNodeByIndex(grid[startPosition].nodeIndex);
            Node<Cell>[] checkpointNodes =
                grid.checkpointNodeIndicies
                    .Where(i => !grid[i].info.blocked)
                    .Select(i => graph.GetNodeByIndex(i))
                    .ToArray();

            Path<Cell> pathThroughCheckpoints = graph.Dijkstra(startNode, checkpointNodes).Subpath(stepsPerRound);
            Dictionary<Node<Cell>, Path<Cell>> allPaths = graph.Dijkstra(startNode, stepsPerRound);
            List<Edge<Cell>> drawnEdges = new();

            // draw paths
            using (vr.Begin()) {
                foreach (Edge<Cell> edge in pathThroughCheckpoints.edges) {
                    Vector3 start = edge.start.ReadData().midpoint;
                    Vector3 end = edge.end.ReadData().midpoint;
                    if (edge.start.ReadData().info.portal) {
                        start += Vector3.up * 0.5f;
                        end += Vector3.up * 0.5f;
                    }
                    vr.Draw(start, end, Color.green, vectorRadius, vectorTipHeight);
                    drawnEdges.Add(edge);
                }
            }

            if (showAllPaths) {
                foreach (Path<Cell> path in allPaths.Values) {
                    using (vr.Begin()) {
                        foreach (Edge<Cell> edge in path.edges) {
                            // don't draw arrows ontop of each other
                            Edge<Cell> reverseEdge = graph.GetEdge(edge.end, edge.start);
                            if (drawnEdges.Contains(edge)) continue;
                            if (reverseEdge != null && pathThroughCheckpoints.HasEdge(reverseEdge)) continue;
                            Vector3 start = edge.start.ReadData().midpoint;
                            Vector3 end = edge.end.ReadData().midpoint;
                            vr.Draw(start, end, Color.gray, vectorRadius, vectorTipHeight);
                            drawnEdges.Add(edge);
                        }
                    }
                }
            }

            // update checkpoint cost numbers
            foreach (int index in grid.checkpointNodeIndicies) {
                Node<Cell> node = graph[grid[index].nodeIndex];
                node.ReadData().SetCostNumber(-1);
            }

            if (grid.checkpointNodeIndicies.Count > 0) {
                uint runningCost = 0;
                int currentCheckPointIndex = 0;
                foreach (Edge<Cell> edge in pathThroughCheckpoints.Subpath(stepsPerRound).edges) {
                    Node<Cell> checkpoint = graph[grid.checkpointNodeIndicies[currentCheckPointIndex]];
                    // if the first checkpoint is the startNode then the cost is zero
                    if (graph[grid[0].nodeIndex] == startNode) startNode.ReadData().SetCostNumber(0);
                    runningCost += edge.weight;

                    Debug.Log($"{edge.end.id} {checkpoint.id}");
                    if (edge.end == checkpoint) {
                        Debug.Log("hello");
                        checkpoint.ReadData().SetCostNumber((int)runningCost);
                        currentCheckPointIndex++;
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
            foreach (int adjacentIndex in adjacent) {
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