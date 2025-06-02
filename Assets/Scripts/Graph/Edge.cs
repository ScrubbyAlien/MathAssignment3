using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge<T>
{
    public readonly Node<T> start;
    public readonly Node<T> end;
    public readonly uint weight;

    public Edge(Node<T> start, Node<T> end, uint weight) {
        this.start = start;
        this.end = end;
        this.weight = weight;
    }

    public Edge(Node<T> start, Node<T> end) : this(start, end, 0) { }
}