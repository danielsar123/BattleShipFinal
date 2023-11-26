using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board 
{
    public GameObject BoardUnitPrefab;

    public GameObject[,] board = new GameObject[10, 10];

    public void ClearBoard()
    {
        for(int i=0; i < 10; i++)
        {
            for(int j =0; j < 10; j++)
            {
                board[i, j] = null;
            }
        }
    }
}
