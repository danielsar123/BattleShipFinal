using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardPlayer :  Board
{
    private Board enemyBoard;

    private bool gameStarted = false; // Flag to check if game has started
    public BoardPlayer(GameObject unitPrefab, Board enemy)
    {
        BoardUnitPrefab = unitPrefab;
        this.enemyBoard = enemy;
        ClearBoard();
    }

    public void CreatePlayerBoard()
    {
        //create a 10x10 board - Board 1
        int row = 1;
        int col = 1;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                //instatiate prefab and place it properly on the scene
                GameObject tmp = GameObject.Instantiate(BoardUnitPrefab, new Vector3(i, 0, j), BoardUnitPrefab.transform.rotation) as GameObject;
                //
                BoardUnit tmpUI = tmp.GetComponentInChildren<BoardUnit>()
;
                string name = string.Format("B1:[{0:00},{1:00}]", row, col);
                tmpUI.tmpBoardUnitLabel.text = name;
                tmpUI.col = j;
                tmpUI.row = i;

                board[i, j] = tmp;
                tmp.name = name;
                col++;
            }
            col = 1;
            row++;
        }
    }
  
 
   
}
