using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Image;

public class OldSudokuGrid
{
    private OldSudokuCell[,] AllCellsGrid = new OldSudokuCell[9, 9];
    private OldSudokuCell[,] AllCellsBox = new OldSudokuCell[9, 9];

    public OldSudokuGrid()
    {
        for (int i = 0; i < 9 * 9; i++) // 9 * 9
        {
            OldSudokuCell cell = new OldSudokuCell(this, i);

            Debug.Assert(cell != null);

            Debug.Assert(cell.indexColumn >= 0 && cell.indexColumn <= 8);

            Debug.Assert(cell.indexLine >= 0 && cell.indexLine <= 8);

            Debug.Assert(cell.indexBox >= 0 && cell.indexBox <= 8);

            Debug.Assert(cell.indexInsideBox >= 0 && cell.indexInsideBox <= 8);

            AllCellsGrid[cell.indexColumn, cell.indexLine] = cell;
            AllCellsBox[cell.indexBox, cell.indexInsideBox] = cell;

            //Debug.Log("############################################### C:" + cell.indexColumn + " L:" + cell.indexLine);
        }
    }

    public OldSudokuGrid(string flatGrid, bool force = false) : this()
    {
        if (force)
            SetForceFromString(flatGrid);
        else
            SetFromString(flatGrid, true);
    }

    public void PropagateSolutionConstraintsToOthersCells(OldSudokuCell cell)
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

    public void RecordSolutionStep(OldSudokuCell sudokuCell)
    {
        // USEFUL ANYMORE ?
    }

    public bool CheckASolutionNumber(OldSudokuCell cell, int solutionNumber)
    {
        // CHECK IN LINES =====================================================
        for (int l = 0; l < 9; l++)
            if (l != cell.indexLine)
            {
                OldSudokuCell c1 = AllCellsGrid[cell.indexColumn, l];
                if (c1.hasOnlyOneSolution && c1.solutionNumber == solutionNumber)
                    return false;
            }

        // CHECK IN COLUMNS ===================================================
        for (int c = 0; c < 9; c++)
            if (c != cell.indexColumn)
            {
                OldSudokuCell c1 = AllCellsGrid[c, cell.indexLine];
                if (c1.hasOnlyOneSolution && c1.solutionNumber == solutionNumber)
                    return false;
            }

        // CHECK IN BOXES =====================================================
        for (int i = 0; i < 9; i++)
            if (i != cell.indexInsideBox)
            {
                OldSudokuCell c1 = AllCellsBox[cell.indexBox, i];
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
                OldSudokuCell cell = AllCellsGrid[c, l];
                sb.Append(cell.hasOnlyOneSolution ? cell.solutionNumber : "-"); // DONT USE <'>
            }

        //Debug.Log(sb.Length + " ► SudokuGrid === " + sb.ToString());

        return sb.ToString();
    }

    public void SetForceFromString(string text)
    {
        char[] sudoku = text.ToCharArray();

        ResetAllCells();

        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
            {
                int number;
                if (int.TryParse(sudoku[l * 9 + c].ToString(), out number))
                {
                    Debug.Assert(number >= 1 && number <= 9);

                    Debug.Assert(AllCellsGrid[c, l] != null);

                    AllCellsGrid[c, l].ForceSet(number);
                }
            }

        //Debug.Log(sudoku.Length + " ► SetForceFromString === " + string.Join(" ", sudoku) + " RES: " + this.ToString() + "\n" + this.ToPrettyTab());
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

                OldSudokuCell cell = AllCellsGrid[c, l];
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

    public OldSudokuCell GetLowestEntropyCell()
    {
        int minEntropy = int.MaxValue;
        OldSudokuCell minEntropyCell = null;

        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
            {
                OldSudokuCell cell = AllCellsGrid[c, l];

                ////////////Debug.Assert(cell.entropy != 0); // PROBLEM !!!

                if (!cell.alreadyParsedBySolver && cell.entropy > 1 && cell.entropy < minEntropy)
                {
                    minEntropy = cell.entropy;
                    minEntropyCell = cell;
                }
            }

        ////Debug.Log("GetLowestEntropyCell: " + minEntropyCell);

        return minEntropyCell; // so, can be null
    }

    public bool IsComplete()
    {
        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
                if (!AllCellsGrid[c, l].hasOnlyOneSolution) return false;

        return true;
    }

    public bool IsValid()
    {
        List<int> numbers = new List<int>();

        for (int l = 0; l < 9; l++)
        {
            numbers.Clear(); // reset before each new COLUMN
            for (int c = 0; c < 9; c++)
            {
                OldSudokuCell cell = AllCellsGrid[c, l];
                int solution = cell.solutionNumber; // 0 if no solution yet
                if (solution != 0)
                {
                    if (numbers.Contains(solution))
                        return false;

                    numbers.Add(solution);
                }
            }
        }

        for (int c = 0; c < 9; c++)
        {
            numbers.Clear(); // reset before each new LINE
            for (int l = 0; l < 9; l++)
            {
                OldSudokuCell cell = AllCellsGrid[c, l];
                int solution = cell.solutionNumber; // 0 if no solution yet
                if (solution != 0)
                {
                    if (numbers.Contains(solution))
                        return false;

                    numbers.Add(solution);
                }
            }
        }

        for (int b = 0; b < 9; b++)
        {
            numbers.Clear(); // reset before each new BOX
            for (int i = 0; i < 9; i++)
            {
                OldSudokuCell cell = AllCellsBox[b, i];
                int solution = cell.solutionNumber; // 0 if no solution yet
                if (solution != 0)
                {
                    if (numbers.Contains(solution))
                        return false;

                    numbers.Add(solution);
                }
            }
        }

        // finally ...
        return true;
    }

    internal string InsertIntoFlatTextGrid(OldSudokuCell cell, int number)
    {
        string flatTextGrid = this.ToString();
        int position = cell.indexLine * 9 + cell.indexColumn;
        string result = flatTextGrid.Substring(0, position) + number + flatTextGrid.Substring(position + 1);

        return result;
    }

    internal void SetAllEntropies()
    {
        for (int l = 0; l < 9; l++)
        {
            for (int c = 0; c < 9; c++)
            {
                OldSudokuCell cell = AllCellsGrid[c, l];
                if (!cell.hasOnlyOneSolution)
                    SetConstraints(cell);
            }
        }
    }

    public void SetConstraints(OldSudokuCell paramCell)
    {
        List<int> numbers = new List<int>();

        for (int c = 0; c < 9; c++)
        {
            if (c == paramCell.indexColumn)
                continue;

            int solution = AllCellsGrid[c, paramCell.indexLine].solutionNumber; // 0 if no solution yet
            if (solution != 0 && !numbers.Contains(solution))
                numbers.Add(solution);
        }

        for (int l = 0; l < 9; l++)
        {
            if (l == paramCell.indexLine)
                continue;

            int solution = AllCellsGrid[paramCell.indexColumn, l].solutionNumber; // 0 if no solution yet
            if (solution != 0 && !numbers.Contains(solution))
                numbers.Add(solution);
        }

        for (int i = 0; i < 9; i++)
        {
            if (i == paramCell.indexInsideBox)
                continue;

            int solution = AllCellsBox[paramCell.indexBox, i].solutionNumber; // 0 if no solution yet
            if (solution != 0 && !numbers.Contains(solution))
                numbers.Add(solution);
        }

        paramCell.SetContraints(numbers);
    }

}
