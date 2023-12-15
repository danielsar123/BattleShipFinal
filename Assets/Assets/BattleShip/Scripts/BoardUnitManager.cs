using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardUnitManager : MonoBehaviour
{
    public GameObject fire;
    public GameObject bomb;
    public UIManager uiManager;
    public CamerasController controller;
    public ShipPanelManager shipPanelManager;
    public delegate void BoardPiecePlaced(int id);
    public static event BoardPiecePlaced OnBoardPiecePlaced;
    public GameObject BoardUnitPrefab;
    public GameObject BoardUnitAttackPrefab;
    public GameObject BlockVisualizerPrefab;
    private bool isAttackPhase = false;
    public BoardPlayer boardPlayer;
    public BoardAI boardEnemy;

   // public int[] ShipSizes = { 2, 3, 3, 4, 5 };
    public int ShipSize = 2;
    public bool Vertical = true;

    [Header("Player Piece Model Prefact Reference")]
    public List<GameObject> boardPiecesPref;

    [Header("----")]
    //public int blockSize = 3;
    //public bool Oerientation = false;

    bool PLACE_BLOCK = true;

    [SerializeField]
    private int currentShipID;

    GameObject tmpHighlight = null;
    RaycastHit tmpHitHighlight;

    GameObject tmpBlockHolder = null;

    private bool OK_TO_PLACE = true;

    [SerializeField]
    private int count = 0;

    bool placeEnemyShips = true;

    GameObject tmpAttackHighlight = null;
    RaycastHit tmpAttackHitHighlight;

    GameObject tmpAttackBlockHolder = null;
    private void OnEnable()
    {
        UIManager.OnChangeShip += UIManager_OnChangeShip;
        UIManager.OnChangeOrientation += UIManager_OnChangeOrientation;
    }

    private void UIManager_OnChangeOrientation(bool Orienation)
    {
        Vertical = !Vertical;
    }

    private void UIManager_OnChangeShip(int id, int size)
    {
        currentShipID = id;
        ShipSize = size;
    }

    private void OnDisable()
    {
        UIManager.OnChangeShip -= UIManager_OnChangeShip;
        UIManager.OnChangeOrientation -= UIManager_OnChangeOrientation;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the enemy board first
        boardEnemy = new BoardAI(BoardUnitAttackPrefab, BlockVisualizerPrefab);
        

        // Now initialize the player board with a reference to the enemy board
        boardPlayer = new BoardPlayer(BoardUnitPrefab, boardEnemy);
        boardPlayer.CreatePlayerBoard();
        boardEnemy.CreateAiBoard();
        boardEnemy.SetBoardPlayer(this);

        currentShipID = 0;
        ShipSize = 0;
    }

    public void StartAttackPlayer()
    {
        uiManager.EnablePieceButtonsForAttackPhase();
        isAttackPhase = true;
    }
    void Update()
    {
        if (isAttackPhase)
        {
            HandlePlayerAttack();
        }
        if (IsBusy)
            return;

        if (count < 5)
        {
            PlacePlayerPieces();
        }
        else 
        {
            if (placeEnemyShips)
            {
                placeEnemyShips = false;
                boardEnemy.PlaceShips();
                StartAttackPlayer();
                
              
            }
        }
        
       
    }

    private void HandlePlayerAttack()
    {
        Debug.Log("HandlePlayerAttack called");
        controller.SwitchToEnemyView();
        if (Input.GetMouseButtonDown(0)) // Check for left mouse button click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if the raycast hit an enemy board unit
                BoardUnit enemyUnit = hit.transform.GetComponent<BoardUnit>();
                if (enemyUnit != null && !enemyUnit.hit) // Additional check to ensure unit hasn't already been hit
                {
                    Debug.Log("Clicked on an enemy board unit!");
                    enemyUnit.ProcessHit(); // Process the hit on the enemy unit

                    // Instantiate the bomb at the clicked position
                    Vector3 bombPosition = new Vector3(hit.point.x, 9, hit.point.z); // Adjust the Y value as needed
                    Instantiate(bomb, bombPosition, Quaternion.identity);

                

                    // Check if a ship has been hit and if it has sunk
                    Ship hitShip = boardEnemy.CheckHit(enemyUnit.row, enemyUnit.col);
                    if (hitShip != null && !hitShip.IsSunk())
                    {
                        // Instantiate fire prefab on the hit ship's position
                        // Instantiate the fire VFX with a delay
                        StartCoroutine(InstantiateFireVFX(hit.transform.position));
                        isAttackPhase = false;
                        StartCoroutine(StartEnemyAttack());
                    }
                    if (hitShip != null)
                    {
                        if (hitShip.IsSunk())
                        {
                            Debug.Log($"{hitShip.Name} has been sunk!");
                            shipPanelManager.UpdateShipPanel(hitShip.Name, true);
                            isAttackPhase = false;
                            StartCoroutine(StartEnemyAttack());
                            // Perform any additional actions needed when a ship is sunk
                        }
                        else
                        {
                            Debug.Log($"{hitShip.Name} has been hit!");
                            isAttackPhase = false;
                            StartCoroutine(StartEnemyAttack());
                        }
                    }
                    else
                    {
                        Debug.Log("Missed all ships.");
                        isAttackPhase = false;
                        StartCoroutine(StartEnemyAttack());

                    }

                    // Additional logic (e.g., check if the game is over, switch turns, etc.)
                }
                else
                {
                    Debug.Log("Clicked, but not on an enemy board unit.");
                }
            }
            else
            {
                Debug.Log("Raycast didn't hit anything when clicked.");
            }
        }
    }
    private IEnumerator StartEnemyAttack()
    {
        // Wait for a second
        yield return new WaitForSeconds(3.5f);
        controller.SwitchToPlayerView();
        boardEnemy.EnemyAttack();
    }
    public void PlayerTurn()
    {
        StartCoroutine(SwitchToPlayerTurn());
    }
    public IEnumerator SwitchToPlayerTurn()
    {
        yield return new WaitForSeconds(3.5f);
        isAttackPhase = true;
    }
    private IEnumerator InstantiateFireVFX(Vector3 position)
    {
        // Wait for a second
        yield return new WaitForSeconds(1.2f);

        // Instantiate the fire VFX
        Instantiate(fire, position, Quaternion.identity);
    }


    private void PlacePlayerPieces()
    {
       
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.mousePosition != null)
        {
            if (Physics.Raycast(ray, out tmpHitHighlight, 100))
            {
                BoardUnit tmpUI = tmpHitHighlight.transform.GetComponent<BoardUnit>();
                if (tmpHitHighlight.transform.tag.Equals("PlayerBoardUnit") && !tmpUI.occupied)
                {
                    BoardUnit boardData = boardPlayer.board[tmpUI.row, tmpUI.col].transform.GetComponent<BoardUnit>();

                    if (tmpHighlight != null)
                    {
                        if (boardData.occupied)
                            tmpHighlight.GetComponent<Renderer>().material.color = Color.red;
                        else
                            tmpHighlight.GetComponent<Renderer>().material.color = Color.white;
                    }

                    if (tmpBlockHolder != null)
                    {
                        Destroy(tmpBlockHolder);
                    }

                    if (PLACE_BLOCK)
                    {
                        tmpBlockHolder = new GameObject();
                        OK_TO_PLACE = true;

                        // Visualization logic for placing the ship...
                        for (int i = 0; i < ShipSize; i++)
                        {
                            int row = Vertical ? tmpUI.row : tmpUI.row + i;
                            int col = Vertical ? tmpUI.col + i : tmpUI.col;

                            if (row >= 10 || col >= 10) continue; // Skip if outside board bounds

                            GameObject visual = GameObject.Instantiate(BlockVisualizerPrefab, new Vector3(row, BlockVisualizerPrefab.transform.position.y, col), BlockVisualizerPrefab.transform.rotation);
                            BoardUnit bpUI = boardPlayer.board[row, col].GetComponentInChildren<BoardUnit>();

                            if (!bpUI.occupied)
                            {
                                visual.GetComponent<Renderer>().material.color = Color.gray; // okay to place
                            }
                            else
                            {
                                visual.GetComponent<Renderer>().material.color = Color.yellow; // not ok
                                OK_TO_PLACE = false;
                            }

                            visual.transform.parent = tmpBlockHolder.transform;
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.transform.tag.Equals("PlayerBoardUnit"))
                {
                    BoardUnit tmpUI = hit.transform.GetComponentInChildren<BoardUnit>();

                    if (PLACE_BLOCK && OK_TO_PLACE && CanPlaceShip(tmpUI.row, tmpUI.col, Vertical, ShipSize))
                    {
                        // Place the ship on the board
                        for (int i = 0; i < ShipSize; i++)
                        {
                            int row = Vertical ? tmpUI.row : tmpUI.row + i;
                            int col = Vertical ? tmpUI.col + i : tmpUI.col;

                            GameObject sB = boardPlayer.board[row, col];
                            BoardUnit bu = sB.transform.GetComponentInChildren<BoardUnit>();
                            bu.occupied = true;
                            bu.GetComponent<MeshRenderer>().material.color = Color.green;
                            boardPlayer.board[row, col] = sB;
                        }

                        CheckWhichShipWasPlaced(tmpUI.row, tmpUI.col); // Existing logic for checking which ship is placed
                        OK_TO_PLACE = true;
                        tmpHighlight = null;
                    }
                    if (count >= 5 && tmpBlockHolder != null)
                    {
                        Destroy(tmpBlockHolder);
                    }
                }
            }
        }
    }

    // New method to check if a ship can be placed
    private bool CanPlaceShip(int startRow, int startCol, bool vertical, int shipSize)
    {
        for (int i = 0; i < shipSize; i++)
        {
            int checkRow = vertical ? startRow : startRow + i;
            int checkCol = vertical ? startCol + i : startCol;

            if (checkRow >= 10 || checkCol >= 10) return false; // Check for board bounds

            BoardUnit unit = boardPlayer.board[checkRow, checkCol].GetComponentInChildren<BoardUnit>();
            if (unit.occupied) return false; // Check if the position is already occupied
        }
        return true; // All positions are free
    }
    private void CheckWhichShipWasPlaced(int row, int col)
    {
        Debug.Log("Attempting to place ship with ID: " + currentShipID + " at position: " + row + ", " + col);
        switch (currentShipID)
        {
            case 0:
                {
                    if (!Vertical)
                    {
                        Debug.Log($"id is {currentShipID}");
                        
                        GameObject testingVisual = GameObject.Instantiate(boardPiecesPref[currentShipID],
                                                   new Vector3(row + 2 , boardPiecesPref[currentShipID].transform.position.y,col),
                                                   boardPiecesPref[currentShipID].transform.rotation) as GameObject;
                        testingVisual.transform.RotateAround(testingVisual.transform.position, Vector3.up, 90.0f);
                   
                        if (testingVisual == null)
                        {
                            Debug.LogError("Failed to instantiate Aircraft Carrier prefab.");
                        }
                    }
                    else
                    {
                        GameObject testingVisual = GameObject.Instantiate(boardPiecesPref[currentShipID],
                                                new Vector3(row, boardPiecesPref[currentShipID].transform.position.y, col +2),
                                                boardPiecesPref[currentShipID].transform.rotation) as GameObject;
                    }
                    count++;
                    break;
                }
            case 1:
                {
                    if (!Vertical)
                    {
                        Debug.Log($"id is {currentShipID}");
                        // place it as vertical
                        GameObject testingVisual = GameObject.Instantiate(boardPiecesPref[currentShipID],
                                                   new Vector3(row + 1.5f, boardPiecesPref[currentShipID].transform.position.y, col),
                                                   boardPiecesPref[currentShipID].transform.rotation) as GameObject;
                        testingVisual.transform.RotateAround(testingVisual.transform.position, Vector3.up, 90.0f);
                    }
                    else
                    {
                        GameObject testingVisual = GameObject.Instantiate(boardPiecesPref[currentShipID],
                                                new Vector3(row, boardPiecesPref[currentShipID].transform.position.y, col + 1.5f),
                                                boardPiecesPref[currentShipID].transform.rotation) as GameObject;
                    }
                    count++;
                    break;
                }
            case 2:
                {
                    if (!Vertical)
                    {
                        Debug.Log($"id is {currentShipID}");
                        // place it as vertical
                        GameObject testingVisual = GameObject.Instantiate(boardPiecesPref[currentShipID],
                                                   new Vector3(row + 1, boardPiecesPref[currentShipID].transform.position.y, col),
                                                   boardPiecesPref[currentShipID].transform.rotation) as GameObject;
                        testingVisual.transform.RotateAround(testingVisual.transform.position, Vector3.up, 90.0f);
                    }
                    else
                    {
                        GameObject testingVisual = GameObject.Instantiate(boardPiecesPref[currentShipID],
                                                new Vector3(row, boardPiecesPref[currentShipID].transform.position.y, col + 1),
                                                boardPiecesPref[currentShipID].transform.rotation) as GameObject;
                    }
                    count++;
                    break;
                }
            case 3:
                {
                    if (!Vertical)
                    {
                        Debug.Log($"id is {currentShipID}");
                        // place it as vertical
                        GameObject testingVisual = GameObject.Instantiate(boardPiecesPref[currentShipID],
                                                   new Vector3(row + 1, boardPiecesPref[currentShipID].transform.position.y, col),
                                                   boardPiecesPref[currentShipID].transform.rotation) as GameObject;
                        testingVisual.transform.RotateAround(testingVisual.transform.position, Vector3.up, 90.0f);
                    }
                    else
                    {
                        GameObject testingVisual = GameObject.Instantiate(boardPiecesPref[currentShipID],
                                                new Vector3(row, boardPiecesPref[currentShipID].transform.position.y, col + 1),
                                                boardPiecesPref[currentShipID].transform.rotation) as GameObject;
                    }
                    count++;
                    break;
                }
            case 4:
                {
                    if (!Vertical)
                    {
                        Debug.Log($"id is {currentShipID}");
                        // place it as vertical
                        GameObject testingVisual = GameObject.Instantiate(boardPiecesPref[currentShipID],
                                                   new Vector3(row + 0.5f, boardPiecesPref[currentShipID].transform.position.y, col),
                                                   boardPiecesPref[currentShipID].transform.rotation) as GameObject;
                        testingVisual.transform.RotateAround(testingVisual.transform.position, Vector3.up, 90.0f);
                    }
                    else
                    {
                        GameObject testingVisual = GameObject.Instantiate(boardPiecesPref[currentShipID],
                                                new Vector3(row, boardPiecesPref[currentShipID].transform.position.y, col + 0.5f),
                                                boardPiecesPref[currentShipID].transform.rotation) as GameObject;
                    }
                    count++;
                    break;
                }

        }
        OnBoardPiecePlaced?.Invoke(currentShipID);
        StartCoroutine(Wait4Me(0.5f));

        // clear internal data
        currentShipID = -1;
        ShipSize = 0;
    }

    public static bool IsBusy = false;
    IEnumerator Wait4Me(float seconds = 0.5f)
    {
        IsBusy = true;
        //Debug.Log("I AM IN WAIT BEFORE WAIT");
        yield return new WaitForSeconds(seconds);
        //Debug.Log("I AM IN WAIT AFTER WAIT");
        IsBusy = false;
    }
}
