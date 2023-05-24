using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using static System.Net.Mime.MediaTypeNames;

public class SudokuSolver : MonoBehaviour
{
    enum GameStates : byte { NONE, INGAME, REPLAYING, SOLVING, WON };
    private static GameStates gameState;

    [SerializeField]
    private SudokuCellUI original;

    public static SudokuCellUI[,] AllCellsGrid = new SudokuCellUI[9, 9];
    public static SudokuCellUI[,] AllCellsBox = new SudokuCellUI[9, 9];

    private static int CurrentGameStep = 999; // for backtracking / cancelling
    private static Tuple<SudokuCellUI, int>[] SudokuStates = new Tuple<SudokuCellUI, int>[9 * 9];

    public static bool IsInGameMode => gameState == GameStates.INGAME;
    public static bool IsInSolvingMode => gameState == GameStates.SOLVING;

    void Start()
    {
        LogFile.InitLogFile();

        gameState = GameStates.NONE;
        CurrentGameStep = 0;

        for (int i = 0; i < 9 * 9; i++) // 9 * 9
        {
            SudokuCellUI cell = Instantiate(original, this.transform);
            cell.SetCoordinates(i);
        }

        original.gameObject.SetActive(false);

        gameState = GameStates.INGAME;
    }


    public static bool CheckFutureSolutionConstraints(SudokuCellUI cell, int solutionNumber)
    {
        // check in line
        for (int l = 0; l < 9; l++)
        {
            if (l != cell.IndexLine)
            {
                SudokuCellUI c2 = AllCellsGrid[cell.IndexColumn, l];
                if (c2.SolutionSet && c2.Solution == solutionNumber)
                    return false;
            }
        }

        // check in column
        for (int c = 0; c < 9; c++)
        {
            if (c != cell.IndexColumn)
            {
                SudokuCellUI c2 = AllCellsGrid[c, cell.IndexLine];
                if (c2.SolutionSet && c2.Solution == solutionNumber)
                    return false;
            }
        }

        // check in box
        for (int i = 0; i < 9; i++)
        {
            if (i != cell.IndexInsideBox)
            {
                SudokuCellUI c2 = AllCellsBox[cell.IndexBox, i];
                if (c2.SolutionSet && c2.Solution == solutionNumber)
                    return false;
            }
        }

        return true;
    }

    public static void PropagateConstraints(SudokuCellUI cell)
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

        //currentSolvingNode = null;

        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
                AllCellsGrid[c, l].Reset();
    }

    public static void BackOneStep()
    {
        // WORKING BUT NOT OPTIMIZED !!!

        if (CurrentGameStep > 0) CurrentGameStep--;

        ReplaySudokuMoves();
    }

    private static void ReplaySudokuMoves()
    {
        gameState = GameStates.REPLAYING;

        ResetAllCells(false);

        for (int step = 0; step < CurrentGameStep; step++)
        {
            // ask the Cell to set a solution, and its will call back Solver to propagate the constraints
            SudokuStates[step].Item1.TrySetASolution(SudokuStates[step].Item2);
        }

        gameState = GameStates.INGAME;
    }

    internal static void RecordSolutionStep(SudokuCellUI sudokuCell)
    {
        if (gameState != GameStates.REPLAYING && gameState != GameStates.WON)
        {
            Debug.Log("SudokuSolver::RecordSolutionStep >>>>> " + sudokuCell.IndexCell + " SOL:" + sudokuCell.Solution);
            SudokuStates[CurrentGameStep++] = new Tuple<SudokuCellUI, int>(sudokuCell, sudokuCell.Solution);

            if (IsGameWon())
                gameState = GameStates.WON;
        }
    }

    private static bool IsGameWon()
    {
        int count = 9 * 9;
        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
                if (AllCellsGrid[c, l].SolutionSet)
                    count--;

        return (count == 0);
    }

    public void SolveOneStep()
    {
        if (gameState == GameStates.REPLAYING || gameState == GameStates.WON) return;
        /*
        if (currentSolvingNode == null) currentSolvingNode = new SolvingNode();
        //        currentSolvingNode.Reset();

        TrySolveCurrentNode();
        */
    }

    public void SolveAll()
    {
        if (IsGameWon()) return;

        gameState = GameStates.SOLVING;

        // THE FIRST NODE, WE SUPPOSE THE CURRENT GRID IS VALID
        SolvingNode currentSolvingNode = new SolvingNode();
        currentSolvingNode.SetGrid(Sudoku2Text());

        maxRecurseSteps = 0; // sanity check

        /*

        while (true)
        {
            currentSolvingNode = TrySolveCurrentNode(currentSolvingNode);

            if (IsGameWon() || --maxSteps == 0 || currentSolvingNode == null) break;  // STOP LOOP
        }
        */
        //////////////////////////////////////Recursive 
    }

    static int maxRecurseSteps = 0; // sanity check
    private static void RecursiveSolveSudokuNode(SolvingNode currentSolvingNode)
    {
        if (currentSolvingNode == null || IsGameWon() || ++maxRecurseSteps > 2000000) return;  // STOP RECURSION

        // 1] seek for lowest entropy in children
        // 2] if not found, this node is INVALID, return

        // 3] relaunch the function by setting ALL available NUMBERS in that found node
        // 5] if game not WON, mark this node as INVALID
        // 6] restart loop @1 (continue to next lowest entropy child)

    }




    private static SolvingNode TrySolveCurrentNode(SolvingNode currentSolvingNode)
    {
        LogFile.WriteString(PrettyDisplaySudoku(Sudoku2Text()));

        currentSolvingNode.FillChildrenWithSolutions(AllCellsGrid);

        SolvingNode lowestEntropyChild = currentSolvingNode.SetSolutionFromLowestEntropyChild();

        if (lowestEntropyChild == null) // FAIL ....................
        {
            currentSolvingNode.SetInvalid(); // MARK AS INVALID
            currentSolvingNode = currentSolvingNode.parentNode; // BACKTRACKING

            // NO !!!!!!!!!!! USE ANOTHER WAY !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // USE STACK ????????????????
            // RESTORE PREVIOUS STATE (inside node ????????????)
            //////////////////////BackOneStep();

            gameState = GameStates.SOLVING;
        }
        else // OK .................................................
        {
            currentSolvingNode = lowestEntropyChild; // CONTINUE
        }

        return currentSolvingNode;
    }

    public static string Sudoku2Text()
    {
        StringBuilder sb = new StringBuilder();
        // check in line
        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
            {
                SudokuCellUI cell = AllCellsGrid[c, l];
                sb.Append(cell.SolutionSet ? cell.Solution : "-"); // DONT USE <'>
            }

        Debug.Log(sb.Length + " ► Sudoku2Text === " + sb.ToString());
        return sb.ToString();
    }

    public static void Text2Sudoku(string text)
    {
        char[] sudoku = text.ToCharArray();
        Debug.Log(sudoku.Length + " ► Text2Sudoku === " + string.Join("", sudoku));
        ResetAllCells(true);

        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
            {
                int number;
                if (int.TryParse(sudoku[l * 9 + c].ToString(), out number))
                    AllCellsGrid[c, l].TrySetASolution(number);
            }
    }

    public static string PrettyDisplaySudoku(string text)
    {
        // 179 │ 180 ┤ 191 ┐ 192 └ 193 ┴ 194 ┬ 195 ├ 196 ─ 197 ┼   217 ┘ 218 ┌ 

        char[] sudoku = text.ToCharArray();
        StringBuilder sb = new StringBuilder();

        sb.Append("┌───┬───┬───┐\n");
        for (int l = 0; l < 9; l++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (c % 3 == 0) sb.Append("│");

                int number;
                if (int.TryParse(sudoku[l * 9 + c].ToString(), out number))
                    sb.Append(number);
                else
                    sb.Append("·");
            }
            sb.Append("│\n");

            if (l < 8 && l % 3 == 2) sb.Append("├───┼───┼───┤\n");
        }
        sb.Append("└───┴───┴───┘");

        return sb.ToString();
    }

    public void Fill()
    {
        Text2Sudoku("-8--------6---53------9-56-------8-2-------4-3-7-2------5-6-98-7--4----3-4---1---");

        // TESTS -------------------------------------------
        string su = PrettyDisplaySudoku(Sudoku2Text());
        Debug.Log(su);
        LogFile.WriteString(su);
    }

    public void old_Fill()
    {
        /*
        ResetAllCells(true);

        AllCellsGrid[1, 0].TrySetASolution(8);

        AllCellsGrid[1, 1].TrySetASolution(6);
        AllCellsGrid[5, 1].TrySetASolution(5);
        AllCellsGrid[6, 1].TrySetASolution(3);

        AllCellsGrid[4, 2].TrySetASolution(9);
        AllCellsGrid[6, 2].TrySetASolution(5);
        AllCellsGrid[7, 2].TrySetASolution(6);

        AllCellsGrid[6, 3].TrySetASolution(8);
        AllCellsGrid[8, 3].TrySetASolution(2);

        AllCellsGrid[7, 4].TrySetASolution(4);

        AllCellsGrid[0, 5].TrySetASolution(3);
        AllCellsGrid[2, 5].TrySetASolution(7);
        AllCellsGrid[4, 5].TrySetASolution(2);

        AllCellsGrid[2, 6].TrySetASolution(5);
        AllCellsGrid[4, 6].TrySetASolution(6);
        AllCellsGrid[6, 6].TrySetASolution(9);
        AllCellsGrid[7, 6].TrySetASolution(8);

        AllCellsGrid[0, 7].TrySetASolution(7);
        AllCellsGrid[3, 7].TrySetASolution(4);
        AllCellsGrid[8, 7].TrySetASolution(3);

        AllCellsGrid[1, 8].TrySetASolution(4);
        AllCellsGrid[5, 8].TrySetASolution(1);
        */
    }

}
