using System;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public partial class SudokuCell : MonoBehaviour
{
    //[SerializeField]
    private TextMeshProUGUI[] TMPnumbers;

    [SerializeField]
    private RectTransform container;

    private bool isConstrained = false;
    private bool[] numberAuthorized;

    public int NumLine { get; private set; }

    public int NumColumn { get; private set; }

    public int NumBox { get; private set; }

    public int NumCell { get; private set; }

    public static SudokuCell[,] AllCellsGrid = new SudokuCell[9, 9];
    public static SudokuCell[] AllCellsBox = new SudokuCell[9];

    internal void SetPosition(int i)
    {
        //foreach (var t in TMPnumbers)
        //{
        //    t.gameObject.SetActive(false);
        //}

        //TMPnumbers[0].gameObject.SetActive(true);
        //TMPnumbers[0].fontSize = 100;
        //TMPnumbers[0].text = ((i / 3) % 3 + (i / 27) * 3).ToString();

        NumLine = (int)(i / 9f);
        NumColumn = i % 9;
        NumBox = ((i / 3) % 3 + (i / 27) * 3);
        NumCell = i;

        AllCellsGrid[NumLine, NumColumn] = AllCellsBox[NumBox] = this;

        this.name = "BOX_" + NumBox + "_CELL_" + NumCell;
    }

    private void Awake()
    {
        if (container == null)
        {
            container = this.GetComponent<RectTransform>();
        }

        TMPnumbers = container.GetComponentsInChildren<TextMeshProUGUI>();

        isConstrained = false;
        numberAuthorized = new bool[9];


        for (byte i = 0; i < 9; i++)
        {
            numberAuthorized[i] = true;
            TMPnumbers[i].name = TMPnumbers[i].text = (i + 1).ToString();
            
            Debug.Log("AddComponent_" + i);
        }

    }

}