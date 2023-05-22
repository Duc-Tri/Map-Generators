using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuSolverLauncher : MonoBehaviour
{
    [SerializeField]
    private SudokuCell original;

    Color[] colors = new Color[] { Color.grey, Color.green, Color.blue, Color.red, Color.white, Color.magenta, Color.black, Color.yellow, Color.cyan };

    List<SudokuCell> cellList;

    void Awake()
    {
        float transStep = 1f / 10f;
        SudokuSolver solver = new SudokuSolver();

        for (int i = 0; i < 9 * 9; i++) // 9 * 9
        {
            SudokuCell cell = Instantiate(original, this.transform);
            cell.SetPosition(i);
            solver.Add(i, cell);

            Color c = colors[(int)(i / 9f) % colors.Length];
            c.a = (1 + i % 9) * transStep;

            cell.GetComponent<Image>().color = c;

            foreach (var t in cell.GetComponentsInChildren<TextMeshProUGUI>())
            {
                t.color = colors[cell.NumBox];
            }
        }

        original.gameObject.SetActive(false);
    }


}
