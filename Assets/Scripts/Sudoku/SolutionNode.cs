using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.Experimental.GraphView;

public class SolutionNode
{
    private enum NodeStates : byte { NONE, INVALID, ONGOING, VALID }

    public SudokuCell cell;
    public int IndexNodeSolution;
    private NodeStates nodeState;

    List<SolutionNode> children = new List<SolutionNode>();

    public SolutionNode()
    {
        nodeState = NodeStates.NONE;
    }
    public SolutionNode(SudokuCell c, int i)
    {
        cell = c;
        IndexNodeSolution = i;
    }

    internal string FillWithSolutions(SudokuCell[,] allCellsGrid)
    {
        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
            {
                SudokuCell cell = allCellsGrid[l, c];
                if (!cell.SolutionSet)
                    for (int n = 0; n < 9; n++)
                        if (cell.numberAuthorized[n])
                            children.Add(new SolutionNode(cell, n));
            }

        string s = "";
        foreach (SolutionNode node in children) s += node.ToString();

        return s;
    }

    internal SolutionNode TestLowestEntropyChild()
    {
        int minEntropy = int.MaxValue;
        SolutionNode minEntropyNode = null;

        // search for minimum entropy ----------------------------------

        foreach (var node in children)
        {
            if (node.cell.Entropy < minEntropy)
            {
                minEntropyNode = node;
                minEntropy = node.cell.Entropy;
            }
        }

        minEntropyNode.cell.SetASolution(minEntropyNode.IndexNodeSolution + 1);

        return minEntropyNode;
    }

    public override String ToString()
    {
        return cell.IndexCell + "►" + IndexNodeSolution + " ";
    }

    internal void Reset()
    {
        cell = null;
        IndexNodeSolution = -1;
        children.Clear();
        nodeState = NodeStates.NONE;
    }
}


