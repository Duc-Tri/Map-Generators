using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using UnityEditor.Experimental.GraphView;

public class SolvingNode
{
    private enum NodeStates : byte { NONE, INVALID, ONGOING, VALID }
    private NodeStates nodeState;
    public bool IsNotInvalid => nodeState != NodeStates.INVALID;

    public int IndexNodeSolution;

    public SolvingNode parentNode;
    List<SolvingNode> children;

    public string flatTextGrid; // WE KEEP THE SUDOKU IN STRING FORM !!!

    public SolvingNode()
    {
        nodeState = NodeStates.NONE;
    }
    public SolvingNode(SolvingNode parent, int i) : base()
    {
        IndexNodeSolution = i;
        parentNode = parent;
    }

    public string FillChildrenWithSolutions(SudokuCellUI[,] allCellsGrid)
    {
        //if (children != null && children.Count > 0) return "ALREADY FILLED"; // ALREADY FILLED

        children = new List<SolvingNode>();

        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
            {
                /*
                SudokuCell cell = allCellsGrid[l, c];
                if (!cell.SolutionSet)
                    for (int n = 0; n < 9; n++)
                        if (cell.possibilities[n])
                            children.Add(new SolvingNode(this, cell, n));
                */
                //////////////////////////////////////////////////////////////////////////////// Î
            }

        string s = "";
        foreach (SolvingNode node in children) s += node.ToString();

        return s;
    }

    public SolvingNode SetSolutionFromLowestEntropyChild()
    {
        nodeState = NodeStates.ONGOING;

        int minEntropy = int.MaxValue;
        SolvingNode minEntropyNode = null;

        while (true)
        {
            // search for minimum entropy =================================
            minEntropyNode = null;
            foreach (var node in children)
                /*
                if (node.IsNotInvalid && node.cell.Entropy < minEntropy)
                {
                    minEntropyNode = node;
                    minEntropy = node.cell.Entropy;
                }
                */

                if (minEntropyNode == null) break; // NO SOLUTION

            // try the solution ===========================================
            bool trySetASolution = false;
            ////////////trySetASolution = minEntropyNode.cell.TrySetASolution(minEntropyNode.IndexNodeSolution + 1);
            ///
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

    /*
    public override String ToString()
    {
        /////////////return cell.IndexCell + "►" + IndexNodeSolution + " ";
    }
    */

    /*
    public void Reset()
    {
        cell = null;
        IndexNodeSolution = -1;
        nodeState = NodeStates.NONE;

        if (children != null) children.Clear();
    }
    */

    public void SetGrid(string s)
    {
        flatTextGrid = s;
    }

    internal string InsertIntoFlatTextGrid(SudokuCell lowestEntropyCell, int number)
    {
        throw new NotImplementedException();
    }
}


