using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WeightedDigraph<T>
{
    private List<Node<T>> nodes;
    private List<Edge<T>> edges;

    public WeightedDigraph() {
        nodes = new List<Node<T>>();
        edges = new List<Edge<T>>();
    }

    public int AddNode(T nodeData) {
        int index = nodes.Count;
        Node<T> node = new Node<T>(nodeData, index);
        nodes.Add(node);
        return index;
    }

    public void AddEdge(int startIndex, int endIndex, uint weight) {
        Node<T> startNode = nodes[startIndex];
        Node<T> endNode = nodes[endIndex];
        edges.Add(new Edge<T>(startNode, endNode, weight));
    }

    [CanBeNull]
    public Edge<T> GetEdge(int startIndex, int endIndex) {
        return GetEdge(GetNodeByIndex(startIndex), GetNodeByIndex(endIndex));
    }
    [CanBeNull]
    public Edge<T> GetEdge(Node<T> start, Node<T> end) {
        List<Edge<T>> edge = OutEdges(start).Where(e => e.end == end).ToList();
        if (edge.Count == 0) return null;
        return edge.First();
    }

    public Node<T> this[int index] => GetNodeByIndex(index);
    public Node<T> GetNodeByIndex(int index) {
        return nodes[index];
    }
    public bool TryGetNodeByIndex(int index, out Node<T> node) {
        if (index >= 0 && index < nodes.Count) {
            node = GetNodeByIndex(index);
            return true;
        }
        node = null;
        return false;
    }
    public int GetIndexByNode(in Node<T> node) {
        TryGetIndexByNode(in node, out int index);
        return index;
    }
    public bool TryGetIndexByNode(in Node<T> node, out int index) {
        if (!nodes.Contains(node)) {
            index = -1;
            return false;
        }
        index = nodes.IndexOf(node);
        return true;
    }

    public List<Edge<T>> InEdges(int index) {
        if (TryGetNodeByIndex(index, out Node<T> node)) {
            return InEdges(node);
        }
        return new();
    }
    public List<Edge<T>> InEdges(Node<T> node) {
        return edges.Where(e => e.end.Equals(node)).ToList();
    }
    public List<Edge<T>> OutEdges(int index) {
        if (TryGetNodeByIndex(index, out Node<T> node)) {
            return OutEdges(node);
        }
        return new();
    }
    public List<Edge<T>> OutEdges(Node<T> node) {
        return edges.Where(e => e.start.Equals(node)).ToList();
    }

    public List<Node<T>> Successors(int index) {
        return OutEdges(index).Select(e => e.end).ToList();
    }
    public List<Node<T>> Successors(Node<T> node) {
        return OutEdges(node).Select(e => e.end).ToList();
    }
    public List<Node<T>> Predeccessors(int index) {
        return InEdges(index).Select(e => e.start).ToList();
    }
    public List<Node<T>> Predeccessors(Node<T> node) {
        return InEdges(node).Select(e => e.start).ToList();
    }

    public int InDegree(int index) {
        return InEdges(index).Count;
    }
    public int OutDegree(int index) {
        return OutEdges(index).Count;
    }

    public Dictionary<Node<T>, Path<T>> Dijkstra(
        Node<T> startNode, Node<T>[] exitNodes, out List<Path<T>> toExits, int max) {
        // set up groups
        Dictionary<Node<T>, uint> frontier =  new();
        Dictionary<Node<T>, uint> visited = new();
        Dictionary<Node<T>, uint> unvisited = new();
        Dictionary<Node<T>, Path<T>> reached = new();
        HashSet<Node<T>> unreachable = new();

        // add all nodes in graph to the unvisited group...
        foreach (Node<T> node in nodes) {
            unvisited.Add(node, uint.MaxValue);
        }
        //...startnode is added to frontier
        frontier.Add(startNode, 0);
        reached.Add(startNode, new Path<T>(new()));

        toExits = new();

        // search for shortest paths
        while (frontier.Count > 0) {
            uint lowestg = uint.MaxValue;
            Node<T> nextNode = null;

            // find node with smallest g-value in frontier
            foreach (KeyValuePair<Node<T>, uint> pair in frontier) {
                if (pair.Value < lowestg) {
                    lowestg = pair.Value;
                    nextNode = pair.Key;
                }
            }

            // check if this node is an exitNode
            if (exitNodes != null && exitNodes.Contains(nextNode)) {
                // if we are relaxing an exit node we have found the shortest path to it
                toExits.Add(reached[nextNode]);
                // if the shortest path to all exit nodes have been found we break;
                if (toExits.Count == exitNodes.Length) break;
            }

            // relax nextNode
            foreach (Edge<T> edge in OutEdges(nextNode)) {
                if (visited.ContainsKey(edge.end) || unreachable.Contains(edge.end)) continue;

                uint currentg = unvisited[edge.end];
                if (currentg > lowestg + edge.weight) {
                    // shorter path has been found
                    unvisited[edge.end] = lowestg + edge.weight;

                    List<Edge<T>> newPath = reached[nextNode].edges.Append(edge).ToList();
                    if (!reached.TryAdd(edge.end, new Path<T>(newPath))) {
                        reached[edge.end] = new Path<T>(newPath);
                    }
                }

                // if the distance to edge.end is no greater than the max then add it to the frontier
                if (unvisited[edge.end] <= max) {
                    if (!frontier.TryAdd(edge.end, unvisited[edge.end])) {
                        // if this node succeeds an already relaxed node, just update its g-value in the frontier
                        frontier[edge.end] = unvisited[edge.end];
                    }
                }
                else {
                    reached.Remove(edge.end);
                    unreachable.Add(edge.end);
                }
            }

            // remove from frontier and mark node as visited
            frontier.Remove(nextNode);
            unvisited.Remove(nextNode);
            visited.Add(nextNode, lowestg);
        }

        return reached;
    }

    /// <summary>
    /// Returns the shortest paths to the exit node. If no such path exists, or the exit node and the startNode are the same
    /// the path returned will be empty.
    /// </summary>
    public Path<T> Dijkstra(Node<T> startNode, Node<T> exitNode, int max = Int32.MaxValue) {
        Dijkstra(startNode, new[] { exitNode }, out List<Path<T>> exitPath, max);
        return exitPath[0];
    }

    /// <summary>
    /// Returns a dictionary of all reachable nodes from the start node and the shortest path there. 
    /// Only nodes whose path cost is less than or equal to max will be returned.
    /// </summary>
    /// <param name="startNode">The node to start searching from.</param>
    /// <param name="max">The maximum path cost to the node</param>
    /// <returns>Dictionary of all reachable nodes as keys and the shortest path to those nodes as values.</returns>
    public Dictionary<Node<T>, Path<T>> Dijkstra(Node<T> startNode, int max) {
        return Dijkstra(startNode, null, out List<Path<T>> _, max);
    }

    /// <summary>
    /// Return the shortest path through the checkpoints in order.
    /// </summary>
    public Path<T> Dijkstra(Node<T> startNode,  Node<T>[] checkPoints) {
        Path<T> pathThroughCheckpoints = new Path<T>(new());

        for (int i = -1; i < checkPoints.Length - 1; i++) {
            Node<T> fromNode;
            Node<T> toNode;
            if (i == -1) fromNode = startNode;
            else fromNode = checkPoints[i];

            toNode = checkPoints[i + 1];

            // append the shortestPath between the previous checkpoint and the next checkpoint
            pathThroughCheckpoints.Append(Dijkstra(fromNode, toNode));
        }

        return pathThroughCheckpoints;
    }
}