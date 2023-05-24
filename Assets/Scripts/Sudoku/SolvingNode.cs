using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using UnityEditor.Experimental.GraphView;

public class SolvingNode
{
    private enum NodeStates : byte { NONE, INVALID, ONGOING, VALID }

    public SudokuCellUI cell;
    public int IndexNodeSolution;
    private NodeStates nodeState;

    public SolvingNode parentNode;
    List<SolvingNode> children;

    private string currentGrid;

    public bool IsNotInvalid => nodeState != NodeStates.INVALID;

    public SolvingNode()
    {
        nodeState = NodeStates.NONE;
    }
    public SolvingNode(SolvingNode parent, SudokuCellUI c, int i)
    {
        cell = c;
        IndexNodeSolution = i;
        parentNode = parent;
    }

    internal string FillChildrenWithSolutions(SudokuCellUI[,] allCellsGrid)
    {
        //if (children != null && children.Count > 0) return "ALREADY FILLED"; // ALREADY FILLED

        children = new List<SolvingNode>();

        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
            {
                SudokuCellUI cell = allCellsGrid[l, c];
                if (!cell.SolutionSet)
                    for (int n = 0; n < 9; n++)
                        if (cell.numberAuthorized[n])
                            children.Add(new SolvingNode(this, cell, n));
            }

        string s = "";
        foreach (SolvingNode node in children) s += node.ToString();

        return s;
    }

    internal SolvingNode SetSolutionFromLowestEntropyChild()
    {
        nodeState = NodeStates.ONGOING;

        int minEntropy = int.MaxValue;
        SolvingNode minEntropyNode = null;

        while (true)
        {
            // search for minimum entropy =================================
            minEntropyNode = null;
            foreach (var node in children)
                if (node.IsNotInvalid && node.cell.Entropy < minEntropy)
                {
                    minEntropyNode = node;
                    minEntropy = node.cell.Entropy;
                }

            if (minEntropyNode == null) break; // NO SOLUTION

            // try the solution ===========================================
            bool trySetASolution = false;
            trySetASolution = minEntropyNode.cell.TrySetASolution(minEntropyNode.IndexNodeSolution + 1);
            if (!trySetASolution)
            {
                minEntropyNode.SetInvalid();
                minEntropy = int.MaxValue; // IMPORTANT !!!
            }

            if (trySetASolution) break;  // A SOLUTION EXISTS
        }

        return minEntropyNode; // may be null !
    }

    public void SetInvalid()
    {
        nodeState = NodeStates.INVALID;
        // reset children
        if (children != null) children.Clear();

        children = null;
    }

    public override String ToString()
    {
        return cell.IndexCell + "►" + IndexNodeSolution + " ";
    }

    internal void Reset()
    {
        cell = null;
        IndexNodeSolution = -1;
        nodeState = NodeStates.NONE;

        if (children != null) children.Clear();
    }

    internal void SetGrid(string s)
    {
        currentGrid = s;
    }
}


