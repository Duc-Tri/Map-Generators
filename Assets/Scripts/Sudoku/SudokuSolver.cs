using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class SudokuSolver : MonoBehaviour
{
    enum GameStates : byte { NONE, INGAME, REPLAYING, SOLVING, WON };
    private static GameStates gameState;

    [SerializeField]
    private SudokuCell original;

    public static SudokuCell[,] AllCellsGrid = new SudokuCell[9, 9];
    public static SudokuCell[,] AllCellsBox = new SudokuCell[9, 9];

    private static int CurrentGameStep = 999; // for backtracking / cancelling
    private static Tuple<SudokuCell, int>[] SudokuStates = new Tuple<SudokuCell, int>[9 * 9];

    public static bool IsInGameMode => gameState == GameStates.INGAME;

    void Start()
    {
        gameState = GameStates.NONE;
        CurrentGameStep = 0;

        for (int i = 0; i < 9 * 9; i++) // 9 * 9
        {
            SudokuCell cell = Instantiate(original, this.transform);
            cell.SetCoordinates(i);
        }

        original.gameObject.SetActive(false);

        gameState = GameStates.INGAME;
    }

    public static void PropagateConstraints(SudokuCell cell)
    {
        if (!cell.SolutionSet) return; // no solution for this cell yet...

        for (int l = 0; l < 9; l++)
        {
            if (l != cell.IndexLine)
                AllCellsGrid[cell.IndexColumn, l].AddConstraint(cell.Solution);
        }

        for (int c = 0; c < 9; c++)
        {
            if (c != cell.IndexColumn)
                AllCellsGrid[c, cell.IndexLine].AddConstraint(cell.Solution);
        }

        for (int i = 0; i < 9; i++)
        {
            if (i != cell.IndexInsideBox)
                AllCellsBox[cell.IndexBox, i].AddConstraint(cell.Solution);
        }
    }

    public static void ResetAllCells(bool resetSteps = true)
    {
        if (resetSteps) CurrentGameStep = 0;

        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
                AllCellsGrid[c, l].Reset();
    }

    public void BackOneStep()
    {
        // WORKING BUT NOT OPTIMIZED !!!

        if (CurrentGameStep > 0) CurrentGameStep--;

        ReplaySudokuMoves();
    }

    private void ReplaySudokuMoves()
    {
        gameState = GameStates.REPLAYING;

        ResetAllCells(false);

        for (int step = 0; step < CurrentGameStep; step++)
        {
            // ask the Cell to set a solution, and its will call back Solver to propagate the constraints
            SudokuStates[step].Item1.SetASolution(SudokuStates[step].Item2);
        }

        gameState = GameStates.INGAME;
    }

    internal static void RecordSolutionStep(SudokuCell sudokuCell)
    {
        if (gameState != GameStates.REPLAYING && gameState != GameStates.WON)
        {
            Debug.Log("SudokuSolver::RecordSolutionStep >>>>> " + sudokuCell.IndexCell + " SOL:" + sudokuCell.Solution);
            SudokuStates[CurrentGameStep++] = new Tuple<SudokuCell, int>(sudokuCell, sudokuCell.Solution);

            if (IsGameWon())
                gameState = GameStates.WON;
        }
    }

    private static bool IsGameWon()
    {
        int solutions = 9 * 9;
        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
                if (AllCellsGrid[c, l].SolutionSet)
                    solutions--;

        return (solutions == 0);
    }

    static SolutionNode firstNode = new SolutionNode();
    public void Solve()
    {
        if (gameState == GameStates.REPLAYING || gameState == GameStates.WON) return;

        firstNode.Reset();
        maxSteps = 9 * 9;
        RecursiveSolve();
    }

    static int maxSteps = 0;
    private static void RecursiveSolve()
    {
        if (IsGameWon() || --maxSteps == 0) return; // STOP RECURSION

        firstNode.FillWithSolutions(AllCellsGrid);
        firstNode = firstNode.TestLowestEntropyChild();
        RecursiveSolve();
    }

    public void Fill()
    {
        AllCellsGrid[1, 0].SetASolution(8);

        AllCellsGrid[1, 1].SetASolution(6);
        AllCellsGrid[5, 1].SetASolution(5);
        AllCellsGrid[6, 1].SetASolution(3);

        AllCellsGrid[4, 2].SetASolution(9);
        AllCellsGrid[6, 2].SetASolution(5);
        AllCellsGrid[7, 2].SetASolution(6);

        AllCellsGrid[6, 3].SetASolution(8);
        AllCellsGrid[8, 3].SetASolution(2);

        AllCellsGrid[7, 4].SetASolution(4);

        AllCellsGrid[0, 5].SetASolution(3);
        AllCellsGrid[2, 5].SetASolution(7);
        AllCellsGrid[4, 5].SetASolution(2);

        AllCellsGrid[2, 6].SetASolution(5);
        AllCellsGrid[4, 6].SetASolution(6);
        AllCellsGrid[6, 6].SetASolution(9);
        AllCellsGrid[7, 6].SetASolution(8);

        AllCellsGrid[0, 7].SetASolution(7);
        AllCellsGrid[3, 7].SetASolution(4);
        AllCellsGrid[8, 7].SetASolution(3);

        AllCellsGrid[1, 8].SetASolution(4);
        AllCellsGrid[5, 8].SetASolution(1);
    }

}
