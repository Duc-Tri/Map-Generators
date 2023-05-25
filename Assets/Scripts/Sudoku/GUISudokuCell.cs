using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// One of the 9*9 cells on the Sudoku
public class GUISudokuCell : MonoBehaviour
{
    private static Color[] colors = new Color[] { Color.grey, Color.green, Color.blue, Color.red, Color.white, Color.magenta, Color.black, Color.yellow, Color.cyan };

    private TextMeshProUGUI[] TMPnumbers;
    private static float originalFontSize;
    private RectTransform container;

    public bool[] possibilities = new bool[9];

    public bool SolutionSet { get; private set; }

    public int IndexLine { get; private set; }

    public int IndexColumn { get; private set; }

    public int IndexBox { get; private set; }

    public int IndexInsideBox { get; private set; }

    public int IndexCell { get; private set; }

    public int Solution
    {
        get
        {
            if (SolutionSet)
                for (int i = 0; i < 9; i++)
                    if (possibilities[i])
                        return (i + 1);

            return -999;
        }
    }

    public int Entropy
    {
        // here, the entropy is just the numbers authorized remaining
        get
        {
            int entropy = 0;
            foreach (var n in possibilities) if (n) entropy++;

            return entropy;
        }
    }

    public void SetCoordinates(int i)
    {
        IndexCell = i;
        IndexLine = (int)(i / 9f);
        IndexColumn = i % 9;
        IndexBox = ((i / 3) % 3 + (i / 27) * 3);
        IndexInsideBox = ((i % 3) + ((i / 9) % 3) * 3);

        GUISudoku.AllCellsGrid[IndexColumn, IndexLine] = this;
        GUISudoku.AllCellsBox[IndexBox, IndexInsideBox] = this;

        this.name = "BOX_" + IndexBox + "_CELL_" + IndexCell;

        ///////////DebugCell();
    }

    private const float TRANS_STEP = 1f / (1 + 9 * 40f); // 1f / 10f;
    private void Start()
    {
        if (container == null) container = this.GetComponent<RectTransform>();

        TMPnumbers = container.GetComponentsInChildren<TextMeshProUGUI>();

        SolutionSet = false;

        for (byte i = 0; i < 9; i++)
        {
            possibilities[i] = true;
            TMPnumbers[i].name = TMPnumbers[i].text = (i + 1).ToString();

            //Debug.Log("AddComponent_" + i);
        }
        originalFontSize = TMPnumbers[0].fontSize;

        // colors ------------------------------------------
        Color color = colors[colors.Length - IndexLine - 1];
        color.a = TRANS_STEP * (IndexColumn + 1) * 13;
        this.GetComponent<Image>().color = color;

        foreach (var t in TMPnumbers)
            t.color = (IndexBox % 2) == 0 ? Color.black : Color.white;   //colors[IndexBox];
    }

    public void DebugCell()
    {
        for (int n = 1; n < 9; n++)
        {
            var t = TMPnumbers[n];
            t.gameObject.SetActive(false);
        }
        TMPnumbers[0].fontSize = 40; // only one remains
        TMPnumbers[0].text = IndexBox + "/" + IndexInsideBox;
    }

    // solutionNumber = 1..9
    public bool TrySetASolution(int solutionNumber)
    {
        ///Debug.Log("SudokuCell::TrySetASolution_C" + IndexCell + "_X" + IndexColumn + "_Y" + IndexLine + "_B" + IndexBox + " >>> " + solutionNumber);
        
        Debug.Assert(solutionNumber >= 1 && solutionNumber <= 9);

        // already marked, stop here !
        if (SolutionSet) return false;

        int index = solutionNumber - 1; // -1 because in real Sudoku, we have 1..9, but the array index is 0..8

        if (!possibilities[index])
        {
            Debug.LogWarning(IndexCell + " ► TrySetASolution IMPOSSIBLE TO SET ########## " + solutionNumber);

            //if (SudokuSolver.IsInSolvingMode) SudokuSolver.InvalidCurrentNode();

            return false;
        }

        if (!GUISudoku.CheckFutureSolutionConstraints(this, solutionNumber))
        {
            Debug.LogWarning(IndexCell + " ► TrySetASolution DONT RESPECT CONSTRAINT ########## " + solutionNumber);

            //if (SudokuSolver.IsInSolvingMode) SudokuSolver.InvalidCurrentNode();

            return false;
        }

        SolutionSet = true;

        // set all numbers NOT authorized except the parameter
        for (int i = 0; i < 9; i++)
            possibilities[i] = (i == index);

        UpdateUI();

        RecordAndPropagateSolution(index);

        return true;
    }

    private void RecordAndPropagateSolution(int index)
    {
        GUISudoku.RecordSolutionStep(this);
        GUISudoku.PropagateConstraints(this);
    }

    // A CONSTRAINT = A NUMBER YOU CANNOT USE ANYMORE
    public void AddConstraint(int number)
    {
        // ALREADY MARKED, STOP HERE ! ----------------------------------------
        if (SolutionSet) return;

        int index = number - 1;

        //Debug.Log(IndexCell + " ► SudokuCell::AddConstraint_N" + number + "_I" + index);

        // IMPOSSIBLE TO SET THIS CONSTRAINT ----------------------------------
        if (!possibilities[index])
        {
            // HAPPENS FREQUENTLY, WHEN WE ALREADY CONSTRAINT THIS IN PREVIOUS LINE, COLUMN OR BOX
            //Debug.LogWarning(IndexCell + " ► IMPOSSIBLE TO SATISFY THIS CONSTRAINT !!! " + number);
            return;
        }

        possibilities[index] = false;

        TMPnumbers[index].gameObject.SetActive(false);

        // CHECK IF ONLY ONE POSSIBILITY REMAINS => WE HAVE A SOLUTION --------
        int indexUniqueSolution = -1;
        for (int i = 0; i < 9; i++)
            if (possibilities[i])
            {
                if (indexUniqueSolution < 0)
                    indexUniqueSolution = i;
                else
                    return; // more than 1 connstraints remaining, we finished our work, back to normal flow ...
            }

        // IF ZERO POSSIBILITY REMAINS => ERROR, IF ONLY ONE => SOLUTION ------
        if (indexUniqueSolution == -1)
        {
            Debug.LogError(IndexCell + " ► ZERO POSSIBILITY REMAINS !!!");
            return;
        }

        // only one number remaining => it's a solution !
        TrySetASolution(indexUniqueSolution + 1);
    }

    public void Reset()
    {
        SolutionSet = false;

        for (int i = 0; i < 9; i++) possibilities[i] = true;

        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int n = 0; n < 9; n++)
        {
            var t = TMPnumbers[n];
            t.gameObject.SetActive(possibilities[n]);
            if (possibilities[n])
            {
                t.fontSize = originalFontSize;
                t.fontStyle = TMPro.FontStyles.Bold;
            }
        }

        if (SolutionSet)
        {
            //Debug.Log(IndexCell + " ► UpdateUI ============= " + Solution);
            TMPnumbers[Solution - 1].fontStyle = TMPro.FontStyles.Normal;
            TMPnumbers[Solution - 1].fontSize = 140;
        }
    }

}
