using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


// One of the 9 numbers of the 81 Sudoku cells
public class GUISudokuTMPNumber : MonoBehaviour, IPointerClickHandler
{
    TextMeshProUGUI TMPtext;
    GUISudokuCell parentCell;
    int number;

    private void Start()
    {
        TMPtext = GetComponent<TextMeshProUGUI>();
        int.TryParse(TMPtext.text, out number);
        parentCell = transform.parent.GetComponent<GUISudokuCell>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GUISudoku.IsInGameMode)
        {
            Debug.Log("SudokuNumber::OnPointerClick PL" + parentCell.IndexLine +
                "_PC" + parentCell.IndexColumn +
                "_PB" + parentCell.IndexBox +
                " === " + TMPtext.text);

            parentCell.TrySetASolution(number);
        }
    }

}

