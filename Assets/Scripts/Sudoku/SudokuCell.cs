using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// One of the 9*9 cells on the Sudoku
public partial class SudokuCell : MonoBehaviour
{
    private static Color[] colors = new Color[] { Color.grey, Color.green, Color.blue, Color.red, Color.white, Color.magenta, Color.black, Color.yellow, Color.cyan };

    private TextMeshProUGUI[] TMPnumbers;
    private static float originalFontSize;
    private RectTransform container;

    public bool[] numberAuthorized = new bool[9];

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
                    if (numberAuthorized[i])
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
            foreach (var n in numberAuthorized) if (n) entropy++;

            return entropy;
        }
    }

    internal void SetCoordinates(int i)
    {
        IndexCell = i;
        IndexLine = (int)(i / 9f);
        IndexColumn = i % 9;
        IndexBox = ((i / 3) % 3 + (i / 27) * 3);
        IndexInsideBox = ((i % 3) + ((i / 9) % 3) * 3);

        SudokuSolver.AllCellsGrid[IndexColumn, IndexLine] = this;
        SudokuSolver.AllCellsBox[IndexBox, IndexInsideBox] = this;

        this.name = "BOX_" + IndexBox + "_CELL_" + IndexCell;

        ///////////DebugCell();
    }

    private const float TRANS_STEP = 1f / (1 + 9 * 3); // 1f / 10f;
    private void Start()
    {
        if (container == null) container = this.GetComponent<RectTransform>();

        TMPnumbers = container.GetComponentsInChildren<TextMeshProUGUI>();

        SolutionSet = false;

        for (byte i = 0; i < 9; i++)
        {
            numberAuthorized[i] = true;
            TMPnumbers[i].name = TMPnumbers[i].text = (i + 1).ToString();

            Debug.Log("AddComponent_" + i);
        }
        originalFontSize = TMPnumbers[0].fontSize;

        // colors ------------------------------------------
        Color color = colors[colors.Length - IndexLine - 1];
        color.a = TRANS_STEP * (IndexColumn + 1) * 3;
        this.GetComponent<Image>().color = color;

        foreach (var t in TMPnumbers)
            t.color = colors[IndexBox];
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
    internal void SetASolution(int solutionNumber)
    {
        Debug.Log("SudokuCell::SetASolution_C" + IndexCell + "_X" + IndexColumn + "_Y" + IndexLine + "_B" + IndexBox + " >>> " + solutionNumber);
        Debug.Assert(solutionNumber >= 1 && solutionNumber <= 9);

        // already marked, stop here !
        if (SolutionSet) return;

        int index = solutionNumber - 1;
        if (!numberAuthorized[index])
        {
            Debug.LogWarning(IndexCell + " ► SetASolution IMPOSSIBLE TO SET ########## " + solutionNumber);
            return;
        }

        SolutionSet = true;

        // set all numbers NOT authorized except the parameter
        for (int i = 0; i < 9; i++)
            numberAuthorized[i] = (i == index); // -1 because in real Sudoku, we have 1..9, but the array index is 0..8

        UpdateUI();

        RecordAndPropagateSolution(index);
    }

    private void RecordAndPropagateSolution(int index)
    {
        SudokuSolver.RecordSolutionStep(this);
        SudokuSolver.PropagateConstraints(this);
    }

    // A CONSTRAINT = A NUMBER YOU CANNOT USE ANYMORE
    internal void AddConstraint(int number)
    {
        // already marked, stop here !
        if (SolutionSet) return;

        int index = number - 1;

        Debug.LogWarning(IndexCell + " ► SudokuCell::AddConstraint_N" + number + "_I" + index);

        numberAuthorized[index] = false;

        TMPnumbers[index].gameObject.SetActive(false);

        int indexUniqueSolution = -1;
        for (int i = 0; i < 9; i++)
            if (numberAuthorized[i])
            {
                if (indexUniqueSolution < 0)
                    indexUniqueSolution = i;
                else
                    return; // more than 1 connstraints remaining ...
            }

        // sanity check ...
        if (indexUniqueSolution == -1)
        {
            Debug.LogError("NO NUMBER SATISFIES CONSTRAINTS !!!!!!!!!!! " + IndexCell);
            return;
        }

        // only one number remaining => it's a solution !
        SetASolution(indexUniqueSolution + 1);
    }

    internal void Reset()
    {
        SolutionSet = false;

        for (int i = 0; i < 9; i++) numberAuthorized[i] = true;

        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int n = 0; n < 9; n++)
        {
            var t = TMPnumbers[n];
            t.gameObject.SetActive(numberAuthorized[n]);
            t.fontSize = originalFontSize;
            t.fontStyle = TMPro.FontStyles.Normal;
        }

        if (SolutionSet)
        {
            Debug.LogWarning(IndexCell + " ► UpdateUI ============= " + Solution);
            TMPnumbers[Solution - 1].fontStyle = TMPro.FontStyles.Bold;
            TMPnumbers[Solution - 1].fontSize = 140;
        }
    }

    //private List<int> solutionsTested = new List<int>();

    internal void TryARandomSolution999()
    {
        if (SolutionSet) return;

        List<int> possibilities = new List<int>();
        for (int n = 0; n < 9; n++)
            if (numberAuthorized[n])
                possibilities.Add(n);

        int randIndexToTest = possibilities[(int)(Random.value * possibilities.Count)];

        //////////// solutionsTested.Add(randIndexToTest);
        //////////RecordAndPropagateSolution(randIndexToTest);
    }

    internal void ResetTriedSolutions9999()
    {
        //////solutionsTested.Clear();
    }
}
