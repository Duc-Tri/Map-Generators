using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Image;

public class SudokuGrid
{
    private SudokuCell[,] AllCellsGrid = new SudokuCell[9, 9];
    private SudokuCell[,] AllCellsBox = new SudokuCell[9, 9];

    public SudokuGrid()
    {
        for (int i = 0; i < 9 * 9; i++) // 9 * 9
        {
            SudokuCell cell = new SudokuCell(this, i);

            Debug.Assert(cell != null);

            Debug.Assert(cell.indexColumn >= 0 && cell.indexColumn <= 8);

            Debug.Assert(cell.indexLine >= 0 && cell.indexLine <= 8);

            Debug.Assert(cell.indexBox >= 0 && cell.indexBox <= 8);

            Debug.Assert(cell.indexInsideBox >= 0 && cell.indexInsideBox <= 8);

            AllCellsGrid[cell.indexColumn, cell.indexLine] = cell;
            AllCellsBox[cell.indexBox, cell.indexInsideBox] = cell;

            Debug.Log("############################################### C:" + cell.indexColumn + " L:" + cell.indexLine);
        }
    }

    public SudokuGrid(string flatGrid) : this()
    {
        SetFromString(flatGrid, false);
    }

    public void PropagateSolutionConstraintsToOthersCells(SudokuCell cell)
    {
        if (!cell.hasOnlyOneSolution) return; // we dont propagate cell without unique solution

        for (int l = 0; l < 9; l++)
        {
            if (l != cell.indexLine)
                AllCellsGrid[cell.indexColumn, l].AddConstraint(cell.solutionNumber);
        }

        for (int c = 0; c < 9; c++)
        {
            if (c != cell.indexColumn)
                AllCellsGrid[c, cell.indexLine].AddConstraint(cell.solutionNumber);
        }

        for (int i = 0; i < 9; i++)
        {
            if (i != cell.indexInsideBox)
                AllCellsBox[cell.indexBox, i].AddConstraint(cell.solutionNumber);
        }
    }

    public void RecordSolutionStep(SudokuCell sudokuCell)
    {
    }

    public bool CheckASolutionNumber(SudokuCell cell, int solutionNumber)
    {
        // CHECK IN LINES =====================================================
        for (int l = 0; l < 9; l++)
            if (l != cell.indexLine)
            {
                SudokuCell c1 = AllCellsGrid[cell.indexColumn, l];
                if (c1.hasOnlyOneSolution && c1.solutionNumber == solutionNumber)
                    return false;
            }

        // CHECK IN COLUMNS ===================================================
        for (int c = 0; c < 9; c++)
            if (c != cell.indexColumn)
            {
                SudokuCell c1 = AllCellsGrid[c, cell.indexLine];
                if (c1.hasOnlyOneSolution && c1.solutionNumber == solutionNumber)
                    return false;
            }

        // CHECK IN BOXES =====================================================
        for (int i = 0; i < 9; i++)
            if (i != cell.indexInsideBox)
            {
                SudokuCell c1 = AllCellsBox[cell.indexBox, i];
                if (c1.hasOnlyOneSolution && c1.solutionNumber == solutionNumber)
                    return false;
            }

        return true;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        // check in line
        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
            {
                SudokuCell cell = AllCellsGrid[c, l];
                sb.Append(cell.hasOnlyOneSolution ? cell.solutionNumber : "-"); // DONT USE <'>
            }

        Debug.Log(sb.Length + " ► SudokuGrid === " + sb.ToString());

        return sb.ToString();
    }

    public void SetFromString(string text, bool resetCells = true)
    {
        char[] sudoku = text.ToCharArray();

        Debug.Log(sudoku.Length + " ► SetFromString === " + string.Join(" ", sudoku));

        if (resetCells) ResetAllCells();

        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
            {
                int number;
                if (int.TryParse(sudoku[l * 9 + c].ToString(), out number))
                {
                    Debug.Assert(number >= 1 && number <= 9);

                    Debug.Assert(AllCellsGrid[c, l] != null);

                    AllCellsGrid[c, l].TrySetASolution(number);
                }
            }
    }

    public void ResetAllCells()
    {
        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
                AllCellsGrid[c, l].Reset();
    }

    string[] entropyToString = { "█", "░", "A", "B", "C", "D", "E", "F", "G" };
    public string ToPrettyTab(bool withEntropyEncoded = false)
    {
        // 179 │ 180 ┤ 191 ┐ 192 └ 193 ┴ 194 ┬ 195 ├ 196 ─ 197 ┼   217 ┘ 218 ┌ 

        // 185 ╣ 186 ║ 187 ╗ 188 ╝ 200 ╚ 201 ╔ 202 ╩ 203 ╦ 204 ╠ 205 ═ 206 ╬

        StringBuilder sb = new StringBuilder();

        sb.Append("╔═══╦═══╦═══╗\n");
        for (int l = 0; l < 9; l++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (c % 3 == 0) sb.Append("║");

                SudokuCell cell = AllCellsGrid[c, l];
                sb.Append(cell.hasOnlyOneSolution ?
                    cell.solutionNumber :
                    (withEntropyEncoded ? entropyToString[cell.entropy].ToLower() : "·"));  // DONT USE <'>
            }
            sb.Append("║\n");

            if (l < 8 && l % 3 == 2) sb.Append("╠═══╬═══╬═══╣\n");
        }
        sb.Append("╚═══╩═══╩═══╝");

        return sb.ToString();
    }

    public SudokuCell GetLowestEntropyCell()
    {
        throw new NotImplementedException();
        return null;
    }

    public bool IsWin()
    {
        throw new NotImplementedException();
        return false;
    }

    public bool IsValid()
    {
        throw new NotImplementedException();
        return false;
    }


}
