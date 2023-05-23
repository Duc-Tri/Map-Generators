using TMPro;
using UnityEngine;

public partial class SudokuCell : MonoBehaviour
{
    //[SerializeField]
    private TextMeshProUGUI[] TMPnumbers;

    [SerializeField]
    private RectTransform container;

    private bool[] numberAuthorized;
    private bool SolutionMarked = false;
    private static float originalFontSize;

    public int IndexLine { get; private set; }

    public int IndexColumn { get; private set; }

    public int IndexBox { get; private set; }

    public int IndexInsideBox { get; private set; }

    public int IndexCell { get; private set; }

    public int Number
    {
        get
        {
            for (int i = 0; i < 9; i++)
            {
                if (numberAuthorized[i])
                    return (i + 1);
            }
            return -1;
        }
    }

    internal void SetPosition(int i)
    {
        IndexCell = i;
        IndexLine = (int)(i / 9f);
        IndexColumn = i % 9;
        IndexBox = ((i / 3) % 3 + (i / 27) * 3);
        IndexInsideBox = ((i % 3) + ((i / 9) % 3) * 3);

        SudokuSolver.AllCellsGrid[IndexLine, IndexColumn] = this;
        SudokuSolver.AllCellsBox[IndexBox, IndexInsideBox] = this;

        this.name = "BOX_" + IndexBox + "_CELL_" + IndexCell;

        ///////////DebugCell();
    }

    private void Awake()
    {
        if (container == null)
        {
            container = this.GetComponent<RectTransform>();
        }

        TMPnumbers = container.GetComponentsInChildren<TextMeshProUGUI>();

        numberAuthorized = new bool[9];

        for (byte i = 0; i < 9; i++)
        {
            numberAuthorized[i] = true;
            TMPnumbers[i].name = TMPnumbers[i].text = (i + 1).ToString();

            Debug.Log("AddComponent_" + i);
        }
        originalFontSize = TMPnumbers[0].fontSize;
    }


    public void DebugCell()
    {
        for (int n = 1; n < 9; n++)
        {
            var t = TMPnumbers[n];
            t.gameObject.SetActive(false);
        }
        TMPnumbers[0].fontSize = 50; // only one remains
        TMPnumbers[0].text = IndexBox + "/" + IndexInsideBox;
    }

    internal void OnClick(SudokuNumber sudokuNumber, int num)
    {
        Debug.Log("OnClick ### " + IndexLine + " / " + IndexColumn + " >> " + IndexBox);

        for (int i = 0; i < 9; i++)
            numberAuthorized[i] = (i == num - 1); // -1 because in real Sudoku, we have 1..9, but the array index is 0..8

        UpdateUI();

        MarkSolution(num - 1);
    }

    private void MarkSolution(int index)
    {
        if (SolutionMarked)
            return;

        SolutionMarked = true;

        TMPnumbers[index].fontSize = 100; // only one remains => it's a solution !
        SudokuSolver.SetConstrains(this);
    }

    private void UpdateUI()
    {
        for (int n = 0; n < 9; n++)
        {
            var t = TMPnumbers[n];
            t.gameObject.SetActive(numberAuthorized[n]);
            TMPnumbers[n].fontSize = originalFontSize; // TODO: make dynamic
        }
    }

    internal void AddConstraint(int number)
    {
        numberAuthorized[number - 1] = false;
        TMPnumbers[number - 1].gameObject.SetActive(false);

        int index = -1;
        for (int i = 0; i < 9; i++)
        {
            if (numberAuthorized[i])
            {
                if (index < 0)
                    index = i;
                else
                    return; // more than 1 connstraints remaining ...
            }
        }

        // sanity check ...
        if (index == -1)
        {
            Debug.LogError("NO NUMBER SATISFIES CONSTRAINTS !!!!!!!!!!! " + IndexCell);
            return;
        }

        MarkSolution(index);
    }

    internal void Reset()
    {
        SolutionMarked = false;

        for (int i = 0; i < 9; i++)
            numberAuthorized[i] = true;

        UpdateUI();
    }

}
