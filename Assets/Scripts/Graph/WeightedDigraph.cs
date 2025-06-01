using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;

public class WeightedDigraph<T>
{
    private List<Node<T>> nodes;
    private List<Edge> edges;

    public WeightedDigraph() {
        nodes = new List<Node<T>>();
        edges = new List<Edge>();
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
        edges.Add(new Edge(startNode, endNode, weight));
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

    public List<Edge> InEdges(int index) {
        if (TryGetNodeByIndex(index, out Node<T> node)) {
            return InEdges(node);
        }
        return new();
    }
    public List<Edge> InEdges(Node<T> node) {
        return edges.Where(e => e.end.Equals(node)).ToList();
    }
    public List<Edge> OutEdges(int index) {
        if (TryGetNodeByIndex(index, out Node<T> node)) {
            return OutEdges(node);
        }
        return new();
    }
    public List<Edge> OutEdges(Node<T> node) {
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

    public struct Edge
    {
        public readonly Node<T> start;
        public readonly Node<T> end;
        public readonly uint weight;

        public Edge(Node<T> start, Node<T> end, uint weight) {
            this.start = start;
            this.end = end;
            this.weight = weight;
        }
    }

    public Dictionary<Node<T>, Path> Dijkstra(Node<T> startNode, Node<T>[] checkPoints) {
        // set up groups
        Dictionary<Node<T>, uint> frontier =  new();
        Dictionary<Node<T>, uint> visited = new();
        Dictionary<Node<T>, uint> unvisited = new();
        Dictionary<Node<T>, Path> reached = new();

        // add all nodes in graph to the unvisited group, startnode is added to frontier
        foreach (Node<T> node in nodes) {
            unvisited.Add(node, uint.MaxValue);
        }
        frontier.Add(startNode, 0);

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

            // relax nextNode

            foreach (Edge edge in OutEdges(nextNode)) {
                if (visited.ContainsKey(edge.end)) continue;
                
                uint currentg = unvisited[edge.end];
                if (currentg > lowestg + edge.weight) {
                    unvisited[edge.end] = lowestg + edge.weight;
                    
                    // update path to edge.end because shorter path has been found
                    List<Edge> newPath = new List<Edge>() { edge };
                    if (reached.ContainsKey(nextNode)) { // this should only be false when nextNode is startNode
                        newPath = reached[nextNode].edges.Append(edge).ToList();
                    }
                    
                    // set new path or update existing one
                    if (!reached.TryAdd(edge.end, new Path(newPath))) {
                        reached[edge.end] = new Path(newPath);
                    }
                }

                
                if (!frontier.TryAdd(edge.end, unvisited[edge.end])) {
                    // if this node succeeds an already relaxed node, just update its g-value in the frontier
                    frontier[edge.end] = unvisited[edge.end];
                }
            }

            // remove from frontier and mark node as visited
            frontier.Remove(nextNode);
            unvisited.Remove(nextNode);
            visited.Add(nextNode, lowestg);
        }

        return reached;
    }

    public class Path
    {
        public Node<T> startNode => edges[0].start;
        public Node<T> endNode => edges[edges.Count - 1].end;
        public List<Edge> edges;

        public uint totalCost => edges
                                 .Select(t => t.weight)
                                 .Aggregate((costPrev, costNext) => costPrev + costNext);

        public Path(List<Edge> edges) {
            this.edges = edges;
        }
    }
}