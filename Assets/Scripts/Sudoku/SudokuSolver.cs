using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuSolver : MonoBehaviour
{
    [SerializeField]
    private SudokuCell original;

    Color[] colors = new Color[] { Color.grey, Color.green, Color.blue, Color.red, Color.white, Color.magenta, Color.black, Color.yellow, Color.cyan };

    public static SudokuCell[,] AllCellsGrid = new SudokuCell[9, 9];
    public static SudokuCell[,] AllCellsBox = new SudokuCell[9, 9];

    public static SudokuSolver Instance;

    void Awake()
    {
        Instance = this;

        float transStep = 1 / 3f; // 1f / 10f;

        for (int i = 0; i < 9 * 9; i++) // 9 * 9
        {
            SudokuCell cell = Instantiate(original, this.transform);
            cell.SetPosition(i);

            Color c = colors[(int)(i / 9f) % colors.Length];
            c.a = transStep; //  (1 + i % 9) * transStep;

            cell.GetComponent<Image>().color = c;

            foreach (var t in cell.GetComponentsInChildren<TextMeshProUGUI>())
            {
                t.color = colors[cell.IndexBox];
            }
        }

        original.gameObject.SetActive(false);
    }

    public static void SetConstrains(SudokuCell cell)
    {
        for (int l = 0; l < 9; l++)
        {
            if (l != cell.IndexLine)
                AllCellsGrid[l, cell.IndexColumn].AddConstraint(cell.Number);
        }

        for (int c = 0; c < 9; c++)
        {
            if (c != cell.IndexColumn)
                AllCellsGrid[cell.IndexLine, c].AddConstraint(cell.Number);
        }

        for (int b = 0; b < 9; b++)
        {
            if (b != cell.IndexInsideBox)
                AllCellsBox[cell.IndexBox, b].AddConstraint(cell.Number);
        }
    }

    public void Reset()
    {
        for (int l = 0; l < 9; l++)
        {
            for (int c = 0; c < 9; c++)
            {
                AllCellsGrid[l, c].Reset();
            }
        }
    }

}
