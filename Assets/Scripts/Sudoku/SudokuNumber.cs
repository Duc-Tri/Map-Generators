using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SudokuNumber : MonoBehaviour, IPointerClickHandler
{
    TextMeshProUGUI TMPtext;
    SudokuCell parentCell;
    int num;

    private void Start()
    {
        TMPtext = GetComponent<TextMeshProUGUI>();
        int.TryParse(TMPtext.text, out num);

        parentCell = transform.parent.GetComponent<SudokuCell>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("L" + parentCell.NumLine +
            "_C" + parentCell.NumColumn +
            "_B" + parentCell.NumBox + " ___ " + TMPtext.text);

        this.gameObject.SetActive(false);
    }

}

