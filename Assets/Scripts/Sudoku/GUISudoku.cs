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

public class GUISudoku : MonoBehaviour
{
    const string easySudoku__ = "4------1-----695-8-6-5-1----267-5483-47-8-----31942-656--2--1-428-----57-5--9----";
    const string easySolution = "495873216312469578768521349926715483547386921831942765673258194289134657154697832";

    const string hardSudoku__ = "-----------1-9--6---4--5--2--2--8--173-------1----93------7---3--9-4---5-47-6-2--";
    const string hardSolution = "983426517521397468674185932492638751735214896168759324856972143219843675347561289";

    const string veryHardSudoku__ = "8..........36......7..9.2...5...7.......457.....1...3...1....68..85...1..9....4..";
    const string veryHardSolution = "812753649943682175675491283154237896369845721287169534521974368438526917796318452";

    const string diabolicSudoku__ = "4---125-3--8-7-----------1-6------9--7--2-1-6-----1-4--4-3-----3----56-2--------9";
    const string diabolicSolution = "497812563158673924236459718614738295973524186825961347742396851389145672561287439";

    enum GameStates : byte { NONE, INGAME, REPLAYING, SOLVING, WON };
    private static GameStates gameState;

    [SerializeField]
    private GUISudokuCell original;

    public static GUISudokuCell[,] AllCellsGrid = new GUISudokuCell[9, 9];
    public static GUISudokuCell[,] AllCellsBox = new GUISudokuCell[9, 9];

    private static int CurrentGameStep = 999; // for backtracking / cancelling
    private static Tuple<GUISudokuCell, int>[] SudokuStates = new Tuple<GUISudokuCell, int>[9 * 9];

    public static bool IsInGameMode => gameState == GameStates.INGAME;
    public static bool IsInSolvingMode => gameState == GameStates.SOLVING;

    void Start()
    {
        LogFile.InitLogFile();

        gameState = GameStates.NONE;
        CurrentGameStep = 0;

        for (int i = 0; i < 9 * 9; i++) // 9 * 9
        {
            GUISudokuCell cell = Instantiate(original, this.transform);
            cell.SetCoordinates(i);
        }

        original.gameObject.SetActive(false);

        gameState = GameStates.INGAME;

        MakeSomeTests();
    }

    private void MakeSomeTests()
    {
        const string invalidGrid = "583671429964285371271394568659147832928539147317826695135762984796458213842913756";
        const string validGrid2 = "581673429269145378473298561694517832812936745357824196135762984728459613946381257";
        const string validGrid = "983426517521397468674185932492638751735214896168759324856972143219843675347561289";
        const string uncertainGrid = "583216479967845321421397568614579832259183647378624195135762984796458213842931756";

        SudokuGrid grid = new SudokuGrid();

        grid.SetFromString(invalidGrid);
        Debug.Log("SHOULD BE INVALID: " + (grid.IsValid() ? "valid" : "invalid"));
        Debug.Assert(!grid.IsValid());

        grid.SetFromString(validGrid);
        Debug.Log("SHOULD BE VALID: " + (grid.IsValid() ? "valid" : "invalid"));
        Debug.Assert(grid.IsValid());

        grid.SetFromString(uncertainGrid);
        Debug.Log("UNCERTAIN: " + (grid.IsValid() ? "valid" : "invalid"));
        Debug.Assert(grid.IsValid());
    }

    public static bool CheckFutureSolutionConstraints(GUISudokuCell cell, int solutionNumber)
    {
        // check in line
        for (int l = 0; l < 9; l++)
        {
            if (l != cell.IndexLine)
            {
                GUISudokuCell c2 = AllCellsGrid[cell.IndexColumn, l];
                if (c2.SolutionSet && c2.Solution == solutionNumber)
                    return false;
            }
        }

        // check in column
        for (int c = 0; c < 9; c++)
        {
            if (c != cell.IndexColumn)
            {
                GUISudokuCell c2 = AllCellsGrid[c, cell.IndexLine];
                if (c2.SolutionSet && c2.Solution == solutionNumber)
                    return false;
            }
        }

        // check in box
        for (int i = 0; i < 9; i++)
        {
            if (i != cell.IndexInsideBox)
            {
                GUISudokuCell c2 = AllCellsBox[cell.IndexBox, i];
                if (c2.SolutionSet && c2.Solution == solutionNumber)
                    return false;
            }
        }

        return true;
    }

    public static void PropagateConstraints(GUISudokuCell cell)
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
        gameState = GameStates.NONE;

        if (resetSteps) CurrentGameStep = 0;

        //currentSolvingNode = null;

        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
                AllCellsGrid[c, l].Reset();

        gameState = GameStates.INGAME;
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

    public static void RecordSolutionStep(GUISudokuCell sudokuCell)
    {
        if (gameState != GameStates.REPLAYING && gameState != GameStates.WON)
        {
            ////Debug.Log("SudokuSolver::RecordSolutionStep >>>>> " + sudokuCell.IndexCell + " SOL:" + sudokuCell.Solution);
            SudokuStates[CurrentGameStep++] = new Tuple<GUISudokuCell, int>(sudokuCell, sudokuCell.Solution);

            if (WTFIsGameWon())
                gameState = GameStates.WON;
        }
    }

    private static bool WTFIsGameWon()
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
        if (gameState == GameStates.WON || gameState == GameStates.SOLVING) return;

        gameState = GameStates.SOLVING;

        // THE FIRST NODE, WE SUPPOSE THE CURRENT GRID IS VALID
        //recurseSteps = 0; // sanity check
        //RecursiveSolveSudoku1(new OldSudokuGrid(Sudoku2Text()));

        SudokuGrid grid = new SudokuGrid(Sudoku2Text());
        grid.Resolve();

        Debug.Log((grid.ToString().Equals(hardSolution) ? "OK ■■■■■■■■■■ " : "KO !!!!!!!!!! ") + grid.ToString());

        LogFile.WriteString(grid.ToPrettyTab());

        gameState = GameStates.INGAME;
    }

    public static string Sudoku2Text()
    {
        StringBuilder sb = new StringBuilder();
        // check in line
        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
            {
                GUISudokuCell cell = AllCellsGrid[c, l];
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

        gameState = GameStates.NONE;

        for (int l = 0; l < 9; l++)
            for (int c = 0; c < 9; c++)
            {
                int number;
                if (int.TryParse(sudoku[l * 9 + c].ToString(), out number))
                    AllCellsGrid[c, l].TrySetASolution(number);
            }

        gameState = GameStates.INGAME;
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
        // DIFFICULT
        //Text2Sudoku("-8--------6---53------9-56-------8-2-------4-3-7-2------5-6-98-7--4----3-4---1---");

        
        Text2Sudoku(diabolicSudoku__);

        string su = PrettyDisplaySudoku(Sudoku2Text());
        Debug.Log(su);

        // REAL SUDOKU ============================================================================
        /*
        SudokuGrid grid = new SudokuGrid();
        grid.SetFromString("-8--------6---53------9-56-------8-2-------4-3-7-2------5-6-98-7--4----3-4---1---");
        Debug.Log(grid.ToPrettyTab(true));
        LogFile.WriteString(grid.ToPrettyTab(true));
        */
    }
  
}
