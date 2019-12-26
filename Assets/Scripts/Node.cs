using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockState
{
    None = 0,
    Obstruct = 1,
    Start = 2,
    End = 3,
}

[Serializable]
public class Node 
{
    public int col, row;
    public int F, G, H;
    public BlockState state;
    public NodeImage parent;

    public static int GetNodeF(Node node)
    {
        return node.G + node.H;
    }

    public static int GetNodeG(Node centerNode, Node otherNode)
    {
        return otherNode.parent.data.G + GetPrice(centerNode, otherNode);
    }

    public static int GetNodeH(Node node, Node endNode)
    {
        return 10 * (Mathf.Abs(endNode.col - node.col) + Mathf.Abs(endNode.row - node.row));
    }

    public static int GetPrice(Node centerNode, Node otherNode)
    {
        if (Mathf.Abs(centerNode.col - otherNode.col) + Mathf.Abs(centerNode.row - otherNode.row) >= 2)
        {
            return 14;
        }
        else
        {
            return 10;
        }
    }

    public static NodeImage GetMinFNode(List<NodeImage> openList)
    {
        if (openList.Count <= 0)
        {
            return null;
        }

        if (openList.Count == 1)
        {
            return openList[0];
        }

        int index = 0;
        int F = openList[0].data.F;

        for (int i = 1; i < openList.Count; i++)
        {
            if (openList[i].data.F < F)
            {
                index = i;
                F = openList[i].data.F;
            }
        }

        return openList[index];
    }

    public static float GetArrowAngle(Node node)
    {
        int x = node.parent.data.col - node.col;
        int y = node.row - node.parent.data.row;
        return (int) Vector2.SignedAngle(Vector2.right, new Vector2(x, y));
    }

    // □为待确认节点，■为障碍节点
    // 当出现如下对角的□时，可选是否跨越■到达另一个□，
    // ■□
    // □
    // 该方法判断对角的两个对角节点是否被障碍所阻碍，即是否存在上述情况
    public static bool IsImpededByObstruct(NodeImage[,] map, Node centerNode, Node otherNode)
    {
        // 先判断对角方向，即
        //    □■      ■□
        // 是 ■□ 还是 □■
        bool flag = (centerNode.col - otherNode.col) * (centerNode.row - otherNode.row) == 1;

        // 获得同时与 centerNode 和 otherNode 两个对角节点相邻的两个节点
        // 假设节点的二维数组索引如下
        // (1, 2)□■(2, 2)
        // (1, 3)■□(2, 3)

        // 那么(1, 3)■中的1和3，即col和row，有
        // (1, 3)■.col = Mathf.FloorToInt(((1, 2)□.col + □(2, 3).col) / 2f)
        // (1, 3)■.row = Mathf.CeilToInt(((1, 2)□.row + □(2, 3).row) / 2f)
        NodeImage obstructTestNode_1 = flag
            ? map[Mathf.FloorToInt((centerNode.col + otherNode.col) / 2f),
                Mathf.CeilToInt((centerNode.row + otherNode.row) / 2f)]
            : map[Mathf.CeilToInt((centerNode.col + otherNode.col) / 2f),
                Mathf.CeilToInt((centerNode.row + otherNode.row) / 2f)];

        NodeImage obstructTestNode_2 = flag
            ? map[Mathf.CeilToInt((centerNode.col + otherNode.col) / 2f),
                Mathf.FloorToInt((centerNode.row + otherNode.row) / 2f)]
            : map[Mathf.FloorToInt((centerNode.col + otherNode.col) / 2f),
                Mathf.FloorToInt((centerNode.row + otherNode.row) / 2f)];

        // 先判断centerNode和otherNode是否对角
        return Mathf.Abs(centerNode.col - otherNode.col) + Mathf.Abs(centerNode.row - otherNode.row) == 2 &&
               (obstructTestNode_1.data.state == BlockState.Obstruct ||
                obstructTestNode_2.data.state == BlockState.Obstruct);
    }

    public void Reset()
    {
        F = 0;
        G = 0;
        H = 0;
        state = BlockState.None;
        parent = null;
    }
}