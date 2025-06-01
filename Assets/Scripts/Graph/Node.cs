using System.Collections;
using System.Collections.Generic;

public class Node<T>
{
    private int id;
    private T data;
    public T ReadData() => data;

    public Node(T nodeData) {
        data = nodeData;
    }

    public Node(T nodeData, int nodeId) : this(nodeData) {
        id = nodeId;
    }
}