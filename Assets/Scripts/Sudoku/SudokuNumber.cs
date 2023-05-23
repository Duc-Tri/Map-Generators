using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


// One of the nine numbers of the 81 Sudoku cells
public class SudokuNumber : MonoBehaviour, IPointerClickHandler
{
    TextMeshProUGUI TMPtext;
    SudokuCell parentCell;
    int num;

    private void Awake()
    {
        TMPtext = GetComponent<TextMeshProUGUI>();
        int.TryParse(TMPtext.text, out num);
        parentCell = transform.parent.GetComponent<SudokuCell>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("L" + parentCell.IndexLine +
            "_C" + parentCell.IndexColumn +
            "_B" + parentCell.IndexBox + " ___ " + TMPtext.text);

        parentCell.OnClick(this, num);
    }

}

