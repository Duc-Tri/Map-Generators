using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SudokuSolverLauncher : MonoBehaviour
{
    [SerializeField]
    private SudokuCell cell;

    Color[] colors = new Color[] { Color.grey, Color.green, Color.blue, Color.red, Color.magenta, Color.white, Color.black, Color.yellow, Color.cyan };


    List<SudokuCell> cellList;



    // Start is called before the first frame update
    void Start()
    {
        float trans = 1f / 9;

        for (int i = 0; i < 9 * 9; i++)
        {
            SudokuCell go = Instantiate(cell, this.transform);
            go.name = "CELL_" + i;
            Color c = colors[((int)(i / 9)) % colors.Length];
            c.a = (1 + i % 9) * trans;

            go.GetComponent<Image>().color = c;
        }
        cell.gameObject.SetActive(false);
    }


}
