using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// One of the 9*9 cells on the Sudoku
public class OldSudokuCell
{
    private OldSudokuGrid sudokuGrid; // the parent grid

    private bool[] possibilities = new bool[9]; // add +1 to index to have numbers
    internal bool alreadyParsedBySolver;

    public int indexLine { get; private set; }

    public int indexColumn { get; private set; }

    public int indexBox { get; private set; }

    public int indexInsideBox { get; private set; }

    public int indexCell { get; private set; }

    public int solutionNumber
    {
        get
        {
            if (hasOnlyOneSolution)
                for (int i = 0; i < 9; i++)
                    if (possibilities[i])
                        return (i + 1);

            return 0; // ERROR !
        }
    }

    public int entropy
    {
        // here, the entropy is just the possibilities count (authorized numbers)
        get
        {
            int entropy = 0;
            foreach (var p in possibilities) if (p) entropy++;

            return entropy;
        }
    }

    public bool hasOnlyOneSolution
    {
        get
        {
            return entropy == 1;
        }
    }

    public OldSudokuCell(OldSudokuGrid container, int indexCell)
    {
        sudokuGrid = container;

        this.indexCell = indexCell;
        indexLine = indexCell / 9;
        indexColumn = indexCell % 9;
        indexBox = ((indexCell / 3) % 3 + (indexCell / 27) * 3);
        indexInsideBox = (indexCell % 3 + ((indexCell / 9) % 3) * 3);

        Reset();
    }

    public void Reset()
    {
        alreadyParsedBySolver = false;
        for (byte i = 0; i < 9; i++) possibilities[i] = true;
    }

    public bool TrySetASolution(int solutionNumber)
    {
        //Debug.Log("SudokuCell::TrySetASolution" + this.ToString() + " ► " + solutionNumber);

        Debug.Assert(solutionNumber >= 1 && solutionNumber <= 9);

        // already marked, stop here !
        if (hasOnlyOneSolution) return false;

        int iPossibilities = solutionNumber - 1; // -1 because in real Sudoku, we have 1..9, but the array index is 0..8

        if (!possibilities[iPossibilities])
        {
            ////Debug.LogWarning(indexCell + " ► TrySetASolution IMPOSSIBLE TO SET ########## " + solutionNumber);
            return false;
        }

        if (!sudokuGrid.CheckASolutionNumber(this, solutionNumber))
        {
            Debug.LogWarning(indexCell + " ► TrySetASolution DONT RESPECT CONSTRAINT ########## " + solutionNumber);
            return false;
        }

        // SET ALL NUMBERS NOT AUTHORIZED EXCEPT THE PARAMETER
        for (int i = 0; i < 9; i++)
            possibilities[i] = (i == iPossibilities);

        RecordAndPropagateSolution();

        return true;
    }

    private void RecordAndPropagateSolution()
    {
        sudokuGrid.RecordSolutionStep(this);
        sudokuGrid.PropagateSolutionConstraintsToOthersCells(this);
    }

    // A CONSTRAINT = A NUMBER YOU CANNOT USE ANYMORE
    public void AddConstraint(int number)
    {
        // ALREADY MARKED, STOP HERE ! ----------------------------------------
        if (hasOnlyOneSolution) return;

        int index = number - 1;

        ////Debug.Log(indexCell + " ► SudokuCell::AddConstraint_N" + number + "_I" + index);

        // IMPOSSIBLE TO SET THIS CONSTRAINT ----------------------------------
        if (!possibilities[index])
        {
            //Debug.LogWarning(indexCell + " ► IMPOSSIBLE TO SATISFY THIS CONSTRAINT !!!");
            return;
        }

        possibilities[index] = false;

        // CHECK IF ONLY ONE POSSIBILITY REMAINS => WE HAVE A SOLUTION --------
        int e = entropy;

        if (e == 0)
        {
            // IF ZERO POSSIBILITY REMAINS => ERROR, IF ONLY ONE => SOLUTION ------
            Debug.LogError(indexCell + " ► ZERO POSSIBILITY REMAINS !!!");
            return;
        }
        else if (e > 1)
            return; // more than 1 possibilities remaining, back to normal flow ...

        // HERE, only one number remaining => it's a solution !
        RecordAndPropagateSolution();
    }

    public void TryARandomSolution999()
    {
        if (hasOnlyOneSolution) return;

        List<int> poss = new List<int>();
        for (int n = 0; n < 9; n++)
            if (possibilities[n])
                poss.Add(n);

        int randIndexToTest = poss[(int)(Random.value * poss.Count)];
    }

    public override string ToString()
    {
        return "C:" + indexCell + "_X:" + indexColumn + "_Y:" + indexLine + "_B:" + indexBox + "/" + indexInsideBox + " [" + string.Join("-", GetPossibilitiesInNumbersList()) + "]";
    }

    internal List<int> GetPossibilitiesInNumbersList()
    {
        List<int> numbers = new List<int>();
        for (int n = 0; n < 9; n++)
            if (possibilities[n]) numbers.Add(n + 1);

        return numbers;
    }

    internal void ForceSet(int number)
    {
        int iPossibilities = number - 1; // -1 because in real Sudoku, we have 1..9, but the array index is 0..8
        for (int i = 0; i < 9; i++)
            possibilities[i] = (i == iPossibilities);
    }

    internal void SetContraints(List<int> constraints)
    {
        //Debug.Log(indexCell + " ► SetContraints " + String.Join("*", constraints));

        for (int i = 0; i < 9; i++) possibilities[i] = !constraints.Contains(i + 1);
    }
}
