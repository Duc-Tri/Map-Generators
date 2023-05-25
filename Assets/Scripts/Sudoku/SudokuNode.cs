using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SudokuNode : IComparable<SudokuNode>
{
    public List<int> possibilities;

    public int gridIndex;
    public int lineIndex;
    public int columnIndex;
    public int boxIndex;
    public int insideBoxIndex;
    public int lastPopValue;

    public SudokuNode()
    {
        gridIndex = -1;
        possibilities = new List<int>();
    }

    public SudokuNode(int index, List<int> poss)
    {
        gridIndex = index;
        lineIndex = index / 9;
        columnIndex = index % 9;
        boxIndex = ((index / 3) % 3 + (index / 27) * 3);
        insideBoxIndex = (index % 3 + ((index / 9) % 3) * 3);

        lastPopValue = 0;

        possibilities = poss;
    }

    // Used by Sort()
    public int CompareTo(SudokuNode other)
    {
        if (other == null) return 1;

        int pCount = possibilities.Count;
        int otherCount = other.possibilities.Count;

        if (otherCount == pCount)
        {
            if (gridIndex > other.gridIndex)
                return 1;

            return -1;
        }

        if (pCount > otherCount) return 1;

        return -1;
    }

    int valueIndex = 0;
    internal int GetNextValue()
    {
        if (valueIndex > possibilities.Count-1)
        {
            valueIndex = 0;
            return 0;
        }

        return possibilities[valueIndex++];
    }
}
