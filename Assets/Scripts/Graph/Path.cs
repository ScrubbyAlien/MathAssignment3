using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Path<T>
{
    public Node<T> startNode {
        get {
            if (isEmpty) {
                throw new Exception("Path is empty and does not have a start node.");
            }
            else return edges[0].start;
        }
    }
    public Node<T> endNode {
        get {
            if (isEmpty) {
                throw new Exception("Path is empty and does not have an end node.");
            }
            else return edges[edges.Count - 1].end;
        }
    }
    public List<Edge<T>> edges;

    public uint totalCost {
        get {
            if (isEmpty) return 0;
            else return edges.Select(t => t.weight).Aggregate((costPrev, costNext) => costPrev + costNext);
        }
    }

    public bool isEmpty => edges.Count == 0;

    public Path(List<Edge<T>> edges) {
        this.edges = edges;
    }

    public void Append(List<Edge<T>> edgeRange) {
        if (isEmpty || edgeRange[0].start == endNode) edges.AddRange(edgeRange);
        else Debug.LogError("appended path start does not match this paths end");
    }

    public void Append(Edge<T> edge) {
        if (isEmpty || edge.start == endNode) edges.Add(edge);
        else {
            Debug.Log($"{edge.start.id} {endNode.id}");
            Debug.LogError("appended path start does not match this paths end");
        }
    }

    public void Append(Path<T> path) {
        if (isEmpty || path.startNode == endNode) edges.AddRange(path.edges);
        else Debug.LogError("appended path start does not match this paths end");
    }

    /// <summary>
    /// Returns a subpath of this path whose totalCost is no more than maximumCost.
    /// </summary>
    public Path<T> Subpath(int maximumCost) {
        Path<T> subpath = new Path<T>(new());
        foreach (Edge<T> edge in edges) {
            if (subpath.totalCost + edge.weight <= maximumCost) {
                subpath.Append(edge);
            }
            else break;
        }
        return subpath;
    }

    public bool HasEdge(Edge<T> edge) => edges.Contains(edge);
}