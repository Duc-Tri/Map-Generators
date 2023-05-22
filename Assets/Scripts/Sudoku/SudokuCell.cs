using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class SudokuCell : MonoBehaviour
{
    //[SerializeField]
    private TextMeshProUGUI[] TMPnumbers;

    [SerializeField]
    private RectTransform container;

    private bool isConstrained = false;
    private List<byte> possibilities;
    private List<byte> impossibilities;

    private void Start()
    {
        if (container == null)
        {
            container = GetComponent<RectTransform>();
        }

        TMPnumbers = container.GetComponentsInChildren<TextMeshProUGUI>();

        isConstrained = false;
        possibilities = new List<byte>();
        for (byte i = 1; i < 10; i++)
        {
            possibilities.Add(i);
            TMPnumbers[i - 1].name = TMPnumbers[i - 1].text = (i).ToString();
            TMPnumbers[i - 1].AddComponent<SudokuNumber>();
        }
    }

    private class SudokuNumber : MonoBehaviour, IPointerClickHandler
    {
        TextMeshProUGUI TMPtext;
        SudokuCell parentCell;

        private void Start()
        {
            TMPtext = GetComponent<TextMeshProUGUI>();
            parentCell = transform.parent.GetComponent<SudokuCell>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log(parentCell.name + " ___ " + TMPtext.text);

        }
    }










}