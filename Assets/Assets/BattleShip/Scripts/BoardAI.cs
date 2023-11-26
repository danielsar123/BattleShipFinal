using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardAI : Board 
{
    GameObject cubePrefab;
    int[] aiShipSizes = new int[5] { 2, 3, 3, 4, 5 };
    public List<Ship> Ships { get; private set; }
    public BoardAI(GameObject unitPrefab, GameObject prefab)
    {
        BoardUnitPrefab = unitPrefab;
        cubePrefab = prefab;
        Ships = new List<Ship>();
        // Initialize ships based on aiShipSizes
        for (int i = 0; i < aiShipSizes.Length; i++)
        {
            Ships.Add(new Ship($"Ship{i}", aiShipSizes[i]));
        }
        ClearBoard();
    }

    private void AssignShipPosition(Ship ship, int row, int col)
    {
        ship.AddPosition(row, col);
        board[row, col].GetComponentInChildren<BoardUnit>().SetShip(ship);
        board[row, col].GetComponentInChildren<BoardUnit>().occupied = true;
    }
    public Ship CheckHit(int row, int col)
    {
        foreach (var ship in Ships)
        {
            if (ship.Positions.Contains(new Vector2Int(row, col)))
            {
                ship.RegisterHit();
                return ship;
            }
        }
        return null; // No ship at this position
    }
    public void CreateAiBoard()
    {
        //create a 10x10 board - Board 2
        int row = 1;
        int col = 1;
        for (int i = 11; i < 21; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                //instatiate prefab and place it properly on the scene
                GameObject tmp = GameObject.Instantiate(BoardUnitPrefab, new Vector3(i, 0, j), BoardUnitPrefab.transform.rotation) as GameObject;

                BoardUnit tmpUI = tmp.GetComponentInChildren<BoardUnit>();
;
                string name = string.Format("B2:[{0:00},{1:00}]", row, col);
                tmpUI.tmpBoardUnitLabel.text = name;
                tmpUI.col = col - 1;
                tmpUI.row = row - 1;

                board[tmpUI.row, tmpUI.col] = tmp;
                tmp.name = name;
                col++;
            }
            col = 1;
            row++;
        }

    }

    // Logic for placing enemy warships
    public void PlaceShips()
    {
        foreach (var ship in Ships)
        {
            bool shipPlaced = false;
            int attempts = 0;
            while (!shipPlaced && attempts < 100)
            {
                int row = Random.Range(0, 10 - ship.Size); // Ensure there is space for the entire ship
                int col = Random.Range(0, 10 - ship.Size); // Ensure there is space for the entire ship
                bool horizontal = Random.Range(0, 2) == 0;

                shipPlaced = CheckBoardForPlacement(row, col, ship.Size, horizontal, ship);
                attempts++;
            }
            if (!shipPlaced)
            {
                Debug.LogWarning($"Failed to place ship: {ship.Name}");
            }
        }
    }

    private bool CheckBoardForPlacement(int row, int col, int size, bool hor, Ship ship)
    {
        // Check boundaries and occupancy
        for (int i = 0; i < size; i++)
        {
            int checkRow = hor ? row : row + i;
            int checkCol = hor ? col + i : col;

            // If we are outside the board or the position is already occupied, return false
            if (checkRow >= 10 || checkCol >= 10 || board[checkRow, checkCol].GetComponentInChildren<BoardUnit>().occupied)
            {
                return false;
            }
        }

        // If we made it here, it's ok to place the ship
        for (int i = 0; i < size; i++)
        {
            int placeRow = hor ? row : row + i;
            int placeCol = hor ? col + i : col;

            AssignShipPosition(ship, placeRow, placeCol);
            board[placeRow, placeCol].GetComponentInChildren<BoardUnit>().occupied = true;
            GameObject visual = GameObject.Instantiate(cubePrefab, new Vector3(placeRow + 11, 0.4f, placeCol), Quaternion.identity);
            visual.GetComponent<Renderer>().material.color = Color.yellow;
        }
        return true;
    }
}
