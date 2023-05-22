using MapGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SudokuSolver
{
    private SudokuCell[,] grid = new SudokuCell[9, 9]; // 9 x 9

    public SudokuSolver()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = new SudokuCell();
            }
        }
    }


}
