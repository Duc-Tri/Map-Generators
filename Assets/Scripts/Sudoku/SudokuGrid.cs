using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SudokuGrid
{
    private int[] grid = new int[9 * 9];
    private List<SudokuNode> nodes;
    private readonly int[,] BoxLookupTab;

    int indexBox(int i) => ((i / 3) % 3 + (i / 27) * 3);
    int indexInsideBox(int i) => (i % 3 + ((i / 9) % 3) * 3);

    //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

    public SudokuGrid()
    {
        // lookup tab to facilitates futures algorithms
        if (BoxLookupTab == null)
        {
            BoxLookupTab = new int[9, 9];
            for (int index = 0; index < 9 * 9; index++)
                BoxLookupTab[indexBox(index), indexInsideBox(index)] = index;
        }
    }

    public SudokuGrid(string flatGrid) : this()
    {
        SetFromString(flatGrid);
    }

    public void SetFromString(string text)
    {
        char[] sudoku = text.ToCharArray();
        Debug.Assert(sudoku.Length == 81);

        for (int i = 0; i < 81; i++)
        {
            int number;
            if (int.TryParse(sudoku[i].ToString(), out number))
            {
                Debug.Assert(number >= 1 && number <= 9);

                grid[i] = number;
            }
            else
                grid[i] = 0;
        }

        Debug.Log(" ► SetFromString === " + string.Join(" ", sudoku) + " RES: " + this.ToString() + "\n" + this.ToPrettyTab());
    }

    // Remove v as possibility in nodes of same LINE, COLUMN or BOX (from gridIndex)
    private void RemovePossibilitesToLineColumnBox(int gridIndex, int val)
    {
        int lineIndex = gridIndex / 9;
        int columnIndex = gridIndex % 9;
        int boxIndex = ((gridIndex / 3) % 3 + (gridIndex / 27) * 3);

        grid[gridIndex] = val;

        foreach (var node in nodes)
        {
            if (node.gridIndex != gridIndex && (node.lineIndex == lineIndex || node.columnIndex == columnIndex || node.boxIndex == boxIndex) &&
                node.possibilities.Contains(val))
            {
                node.possibilities.Remove(val);

                // ONLY ONE POSSIBILITY REMAINING ? IT'S A SOLUTION !!! -------
                if (node.possibilities.Count == 1)
                {
                    int otherVal = node.possibilities[0];
                    node.possibilities.Clear();
                    RemovePossibilitesToLineColumnBox(node.gridIndex, otherVal);
                }
            }
        }

    }

    // Construct (and constraint) possibilities for empty cell, create a node to wrap it
    private void ConstructAllPossibilitiesNodes()
    {
        nodes = new List<SudokuNode>();

        for (int index = 0; index < 81; index++)
            if (grid[index] == 0)
            {
                List<int> possibilities = ContructNodePossibilities(index);

                Debug.Assert(possibilities.Count > 0);

                if (possibilities.Count > 1)
                {
                    SudokuNode node = new SudokuNode(index, possibilities);
                    nodes.Add(node);
                }
                else // ONLY ONE POSSIBILITY = SOLUTION, PROPAGATE TO LINE, COLUMN, BOX
                {
                    grid[index] = possibilities[0];
                    RemovePossibilitesToLineColumnBox(index, possibilities[0]);
                }
            }

        // CLEAN NODES LIST FROM POSSIBILITIES ZERO ---------------------------
        for (int i = nodes.Count - 1; i >= 0; i--)
            if (nodes[i].possibilities.Count == 0)
                nodes.RemoveAt(i);

        // HERE, OPTIMIZED NODES, SORTING FOR ENTROPY -------------------------
        nodes.Sort();

        foreach (var node in nodes)
            Debug.Log(node.gridIndex + " ■■■■■ [" + string.Join(", ", node.possibilities) + "]");
    }

    // Tell if the grid respect SUDOKU constraints (can consider 0 as valid or not)
    public bool IsValid(bool zeroIsInvalid = false)
    {
        List<int> numbers = new List<int>();

        for (int offset = 0; offset < 81; offset += 9)
        {
            numbers.Clear(); // reset before each new COLUMN
            for (int c = 0; c < 9; c++)
            {
                int val = grid[offset + c];
                if (val != 0) // 0 if no solution yet
                {
                    if (numbers.Contains(val)) return false;

                    numbers.Add(val);
                }
                else
                    if (zeroIsInvalid) return false;
            }
        }

        for (int c = 0; c < 9; c++)
        {
            numbers.Clear(); // reset before each new LINE
            for (int offset = 0; offset < 81; offset += 9)
            {
                int val = grid[offset + c];
                if (val != 0) // 0 if no solution yet
                {
                    if (numbers.Contains(val)) return false;

                    numbers.Add(val);
                }
                else
                    if (zeroIsInvalid) return false;
            }
        }

        for (int b = 0; b < 9; b++)
        {
            numbers.Clear(); // reset before each new BOX
            for (int i = 0; i < 9; i++)
            {
                int val = grid[BoxLookupTab[b, i]];

                if (val != 0) // 0 if no solution yet
                {
                    if (numbers.Contains(val)) return false;
                    numbers.Add(val);
                }
                else
                    if (zeroIsInvalid) return false;
            }
        }

        // finally ...
        return true;
    }

    // Give the possibilities of one cell, with the constraints of its LINE, COLUMN and BOX
    private List<int> ContructNodePossibilities(int gridIndex)
    {
        List<int> possibilities = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        int lineIndex = gridIndex / 9;
        int columnIndex = gridIndex % 9;
        int boxIndex = ((gridIndex / 3) % 3 + (gridIndex / 27) * 3);
        int insideBoxIndex = (gridIndex % 3 + ((gridIndex / 9) % 3) * 3);

        int offset = lineIndex * 9;
        for (int c = 0; c < 9; c++)
        {
            if (offset + c == gridIndex) continue;

            int gridVal = grid[offset + c];

            if (gridVal != 0 && possibilities.Contains(gridVal)) // 0 if no solution yet
                possibilities.Remove(gridVal);
        }

        for (int lineOffset = 0; lineOffset < 81; lineOffset += 9)
        {
            if (lineOffset + columnIndex == gridIndex) continue;

            int gridVal = grid[lineOffset + columnIndex];

            if (gridVal != 0 && possibilities.Contains(gridVal)) // 0 if no solution yet
                possibilities.Remove(gridVal);
        }

        for (int i = 0; i < 9; i++)
        {
            if (insideBoxIndex == i) continue;

            int gridVal = grid[BoxLookupTab[boxIndex, i]];

            if (gridVal != 0 && possibilities.Contains(gridVal)) // 0 if no solution yet
                possibilities.Remove(gridVal);
        }

        return possibilities;
    }

    public string ToPrettyTab(bool withEntropyEncoded = false)
    {
        // 179 │ 180 ┤ 191 ┐ 192 └ 193 ┴ 194 ┬ 195 ├ 196 ─ 197 ┼   217 ┘ 218 ┌ 
        // 185 ╣ 186 ║ 187 ╗ 188 ╝ 200 ╚ 201 ╔ 202 ╩ 203 ╦ 204 ╠ 205 ═ 206 ╬

        StringBuilder sb = new StringBuilder();

        sb.Append("╔═══╦═══╦═══╗\n");
        for (int index = 0; index < 81; index++)
        {
            if (index > 1)
            {
                if (index % 9 == 0)
                    sb.Append("║\n");

                if (index % (9 * 3) == 0)
                    sb.Append("╠═══╬═══╬═══╣\n");
            }

            if (index % 3 == 0) sb.Append("║");

            int cell = grid[index];
            sb.Append(cell > 0 ? cell : "·");  // DONT USE <'>
        }
        sb.Append("║\n╚═══╩═══╩═══╝");

        return sb.ToString();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < 81; i++)
        {
            int cell = grid[i];
            sb.Append(cell > 0 ? cell : "-"); // DONT USE <'>
        }

        //Debug.Log(sb.Length + " ► SudokuGrid === " + sb.ToString());

        return sb.ToString();
    }

    private bool IsValidLineColumnBox(int gridIndex, int val)
    {
        int lineIndex = gridIndex / 9;
        int columnIndex = gridIndex % 9;
        int boxIndex = ((gridIndex / 3) % 3 + (gridIndex / 27) * 3);
        int insideBoxIndex = (gridIndex % 3 + ((gridIndex / 9) % 3) * 3);

        int offset = lineIndex * 9;
        for (int c = 0; c < 9; c++)
        {
            if (offset + c == gridIndex) continue;

            if (grid[offset + c] == val) return false;
        }

        for (int lineOffset = 0; lineOffset < 81; lineOffset += 9)
        {
            if (lineOffset + columnIndex == gridIndex) continue;

            if (grid[lineOffset + columnIndex] == val) return false;
        }

        for (int i = 0; i < 9; i++)
        {
            if (insideBoxIndex == i) continue;

            if (grid[BoxLookupTab[boxIndex, i]] == val) return false;
        }

        // finally
        return true;
    }

    private bool GridIsComplete()
    {
        foreach (int v in grid) if (v == 0) return false;

        return true;
    }

    public bool Resolve()
    {
        ConstructAllPossibilitiesNodes();

        if (nodes.Count == 0) // HAPPENS IN VERY EASY GRID, DUe TO COSNTRAINTS AND AUTO-FILL GRID !!!
            return IsValid(true);

        int currentNodeIndex = 0;
        int val;
        SudokuNode currentNode;
        int maxNodeReached = 0; // pour optimiser l'annulation des valeurs des noeuds suivants

        while (!GridIsComplete())
        {
            currentNode = nodes[currentNodeIndex];

            bool goToNextNode = false;
            do
            {
                val = currentNode.GetNextValue();
                if (val == 0)
                {
                    // plus de valeur disponible, on reboucle, et on remonte vers le noeud précédent
                    grid[currentNode.gridIndex] = 0;
                    currentNodeIndex--;

                    if (currentNodeIndex < 0) return false; // désespoir !!!!!

                    /*
                    for (int subnodeI = currentNodeIndex + 1; subnodeI <= maxNodeReached; subnodeI++)
                        grid[nodes[subnodeI].gridIndex] = 0;
                    */

                    goToNextNode = true;
                }
                else
                {
                    // si c 'est bon, et on teste la validité
                    if (IsValidLineColumnBox(currentNode.gridIndex, val))
                    {
                        // si valide, on affectue la grille  on passe au noeud suivant
                        grid[currentNode.gridIndex] = val;
                        currentNodeIndex++;

                        //Debug.Log(currentNode.gridIndex + " ►►► " + val);

                        if (currentNodeIndex >= nodes.Count) return true; // résultat ?????

                        if (currentNodeIndex > maxNodeReached)
                            maxNodeReached = currentNodeIndex;

                        goToNextNode = true;
                    }

                    // sinon, on prend la valeur suivante du même noeud, au tour suivant
                }
            }
            while (!goToNextNode);

        }

        return false;
    }

}
