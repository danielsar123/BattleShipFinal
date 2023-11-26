using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardUnitManager : MonoBehaviour
{
    public CamerasController controller;
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

        currentShipID = 0;
        ShipSize = 0;
    }

    public void StartAttackPlayer()
    {
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

                    // Check if a ship has been hit and if it has sunk
                    Ship hitShip = boardEnemy.CheckHit(enemyUnit.row, enemyUnit.col);
                    if (hitShip != null)
                    {
                        if (hitShip.IsSunk())
                        {
                            Debug.Log($"{hitShip.Name} has been sunk!");
                            // Perform any additional actions needed when a ship is sunk
                        }
                        else
                        {
                            Debug.Log($"{hitShip.Name} has been hit!");
                        }
                    }
                    else
                    {
                        Debug.Log("Missed all ships.");
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
    private void PlacePlayerPieces()
    {
        // capture the mouse position and cast a ray to see what object we hit
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

                        if (!Vertical && (tmpUI.row <= 10 - ShipSize))
                        {
                            for (int i = 0; i < ShipSize; i++)
                            {
                                GameObject visual = GameObject.Instantiate(BlockVisualizerPrefab, new Vector3(tmpUI.row + i,
                                    BlockVisualizerPrefab.transform.position.y, tmpUI.col), BlockVisualizerPrefab.transform.rotation) as GameObject;
                                GameObject bp = boardPlayer.board[tmpUI.row + i, tmpUI.col];
                                BoardUnit bpUI = bp.GetComponentInChildren<BoardUnit>();
                                if (!bpUI.occupied)
                                {
                                    visual.GetComponent<Renderer>().material.color = Color.grey;
                                }
                                else
                                {
                                    //visual.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                                    visual.GetComponent<Renderer>().material.color = Color.yellow; //not ok
                                    OK_TO_PLACE = false;
                                }

                                visual.transform.parent = this.tmpBlockHolder.transform;
                            }
                        }
                        if (Vertical && (tmpUI.col <= 10 - ShipSize))
                        {

                            for (int i = 0; i < ShipSize; i++)
                            {

                                GameObject visual = GameObject.Instantiate(BlockVisualizerPrefab, new Vector3(tmpUI.row,
                                    BlockVisualizerPrefab.transform.position.y, tmpUI.col + i), BlockVisualizerPrefab.transform.rotation) as GameObject;
                                GameObject bp = boardPlayer.board[tmpUI.row, tmpUI.col + i];
                                BoardUnit bpUI = bp.GetComponentInChildren<BoardUnit>();
                                if (!bpUI.occupied)
                                {
                                    visual.GetComponent<Renderer>().material.color = Color.gray; //okay to place
                                }
                                else
                                {
                                    //visual.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                                    visual.GetComponent<Renderer>().material.color = Color.yellow; //not ok
                                    OK_TO_PLACE = false;
                                }
                                visual.transform.parent = this.tmpBlockHolder.transform;
                            }

                        }

                    }
                }
            }
        }
        if (Input.GetMouseButton(0))
        {
            // capture the mouse position and cast a ray to see what object we hit
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.transform.tag.Equals("PlayerBoardUnit"))
                {
                    BoardUnit tmpUI = hit.transform.GetComponentInChildren<BoardUnit>();

                    if (PLACE_BLOCK && OK_TO_PLACE)
                    {
                        if (!Vertical)
                        {
                            for (int i = 0; i < ShipSize; i++)
                            {
                                GameObject sB = boardPlayer.board[tmpUI.row + i, tmpUI.col];

                                BoardUnit bu = sB.transform.GetComponentInChildren<BoardUnit>();
                                bu.occupied = true;
                                //     bu.CubePrefab.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
                                //    bu.CubePrefab.gameObject.SetActive(true);
                                bu.GetComponent<MeshRenderer>().material.color = Color.green;
                                boardPlayer.board[tmpUI.row + i, tmpUI.col] = sB;

                            }
                        }

                        if (Vertical)
                        {
                            for (int i = 0; i < ShipSize; i++)
                            {
                                GameObject sB = boardPlayer.board[tmpUI.row, tmpUI.col + i];

                                BoardUnit bu = sB.transform.GetComponentInChildren<BoardUnit>();
                                bu.occupied = true;
                                bu.GetComponent<MeshRenderer>().material.color = Color.green;

                                boardPlayer.board[tmpUI.row, tmpUI.col + i] = sB;

                            }
                        }
                        // check which ship was placed
                        CheckWhichShipWasPlaced(tmpUI.row, tmpUI.col);

                        OK_TO_PLACE = true;
                        tmpHighlight = null;
                    }
                    // check and destroy the temp block holder
                    // this is used to destroy the last tmp blocks after we have placed the last block group on board
                    if (count >= 5)
                    {
                        if (tmpBlockHolder != null)
                        {
                            Destroy(tmpBlockHolder);
                        }
                    }
                }
            }
        }

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
